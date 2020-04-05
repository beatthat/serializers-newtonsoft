using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BeatThat.Serializers.Newtonsoft
{
    public class JsonNetSerializer<T> : ReaderBase<T>, Serializer<T>
    {
        public static Serializer<T> SHARED_INSTANCE = new JsonNetSerializer<T>();
        public static SerializerFactory<T> SHARED_INSTANCE_FACTORY = new SingleInstanceFactory<T>(SHARED_INSTANCE);

        private static JsonSerializer CreateSharedSerializer()
        {
            var serializer = new JsonSerializer();
            serializer.Converters.Clear();
            serializer.Converters.Add(new DictionaryConverter());
            return serializer;
        }

        private static readonly JsonSerializer SHARED_SERIALIZER = CreateSharedSerializer();

        public const int DEFAULT_BUFFER_SIZE = 1024;

        public JsonNetSerializer() : this(null) { } // need a zero-arg constructor in case created by pool

        public JsonNetSerializer(Encoding encoding = null, int bufferSize = DEFAULT_BUFFER_SIZE)
        {
            this.encoding = encoding ?? Encoding.UTF8;
            this.bufferSize = bufferSize;
        }

        public Encoding encoding { get; private set; }
        public int bufferSize { get; private set; }

        public override bool isThreadsafe { get { return true; } }

        override public T ReadOne(Stream s)
        {
            return SHARED_SERIALIZER.Deserialize<T>(new JsonTextReader(new StreamReader(s)));
        }

        override public T ReadOne(Stream s, ref T toObject)
        {
            toObject = SHARED_SERIALIZER.Deserialize<T>(new JsonTextReader(new StreamReader(s)));
            return toObject;
            // TODO: use streams directly
            //string json = null;
            //using (var r = new StreamReader(s))
            //{
            //    json = r.ReadToEnd();
            //}
            //return JsonToItem(json, ref toObject);
        }

        override public T[] ReadArray(Stream s)
        {
            string json = null;
            using (var r = new StreamReader(s))
            {
                json = r.ReadToEnd();
            }
            return JsonToArray(json);
        }

        virtual public T[] JsonToArray(string json)
        {
            throw new NotSupportedException();
        }

        public T JsonToItem(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public void WriteOne(Stream s, T obj)
        {
#if NET_4_6
            using (var w = new JsonTextWriter(new StreamWriter(s, this.encoding, this.bufferSize, leaveOpen: true)))
            {
                SHARED_SERIALIZER.Serialize(w, obj, typeof(T));
                w.Flush();
            }
#else
            var json = JsonConvert.SerializeObject(obj);
            var bytes = this.encoding.GetBytes(obj);
            s.Write(bytes, 0, bytes.Length);
#endif
        }

        public class DictionaryConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) { this.WriteValue(writer, value); }

            private void WriteValue(JsonWriter writer, object value)
            {
                var t = JToken.FromObject(value);
                switch (t.Type)
                {
                    case JTokenType.Object:
                        this.WriteObject(writer, value);
                        break;
                    case JTokenType.Array:
                        this.WriteArray(writer, value);
                        break;
                    default:
                        writer.WriteValue(value);
                        break;
                }
            }

            private void WriteObject(JsonWriter writer, object value)
            {
                writer.WriteStartObject();
                var obj = value as IDictionary<string, object>;
                foreach (var kvp in obj)
                {
                    writer.WritePropertyName(kvp.Key);
                    this.WriteValue(writer, kvp.Value);
                }
                writer.WriteEndObject();
            }

            private void WriteArray(JsonWriter writer, object value)
            {
                writer.WriteStartArray();
                var array = value as IEnumerable<object>;
                foreach (var o in array)
                {
                    this.WriteValue(writer, o);
                }
                writer.WriteEndArray();
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                return ReadValue(reader);
            }

            private object ReadValue(JsonReader reader)
            {
                while (reader.TokenType == JsonToken.Comment)
                {
                    if (!reader.Read()) throw new JsonSerializationException("Unexpected Token when converting IDictionary<string, object>");
                }
                switch (reader.TokenType)
                {
                    case JsonToken.StartObject:
                        return ReadObject(reader);
                    case JsonToken.StartArray:
                        return this.ReadArray(reader);
                    case JsonToken.Integer:
                    case JsonToken.Float:
                    case JsonToken.String:
                    case JsonToken.Boolean:
                    case JsonToken.Undefined:
                    case JsonToken.Null:
                    case JsonToken.Date:
                    case JsonToken.Bytes:
                        return reader.Value;
                    default:
                        throw new JsonSerializationException
                            (string.Format("Unexpected token when converting IDictionary<string, object>: {0}", reader.TokenType));
                }
            }

            private object ReadArray(JsonReader reader)
            {
                IList<object> list = new List<object>();

                while (reader.Read())
                {
                    switch (reader.TokenType)
                    {
                        case JsonToken.Comment:
                            break;
                        default:
                            var v = ReadValue(reader);

                            list.Add(v);
                            break;
                        case JsonToken.EndArray:
                            return list;
                    }
                }

                throw new JsonSerializationException("Unexpected end when reading IDictionary<string, object>");
            }

            private object ReadObject(JsonReader reader)
            {
                var obj = new Dictionary<string, object>();

                while (reader.Read())
                {
                    switch (reader.TokenType)
                    {
                        case JsonToken.PropertyName:
                            var propertyName = reader.Value.ToString();

                            if (!reader.Read())
                            {
                                throw new JsonSerializationException("Unexpected end when reading IDictionary<string, object>");
                            }

                            var v = ReadValue(reader);

                            obj[propertyName] = v;
                            break;
                        case JsonToken.Comment:
                            break;
                        case JsonToken.EndObject:
                            return obj;
                    }
                }

                throw new JsonSerializationException("Unexpected end when reading IDictionary<string, object>");
            }

            public override bool CanConvert(Type objectType) { return typeof(IDictionary<string, object>).IsAssignableFrom(objectType); }
        }
    }
}
