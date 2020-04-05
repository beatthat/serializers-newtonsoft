namespace BeatThat.Serializers.Newtonsoft
{
    [CanReadAndWriteArrays]
    public class JsonNetSerializerFactory : SerializerFactory
    {
        public Serializer<T> Create<T>()
        {
            return JsonNetSerializer<T>.SHARED_INSTANCE;
        }
    }
}