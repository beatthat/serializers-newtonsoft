An implementation of `BeatThat.Serializers` with more full featured json support. The default `BeatThat.Serializers.JsonSerializer` uses Unity's `JSONUtility` class to read and write json. Unity's JSONUtility is efficient, but it has a number of serious limitations, including these:

- no support for `Dictionary` types
- no support for dates
- no support for C# properties (as opposed to public fields)
- top level object cannot be an array (must be an object)

## Install

From your unity project folder:

    npm init
    npm install beatthat/serializers-newtonsoft --save

## USAGE

#### Read an item from a stream

The example below shows reading a single json item from a stream. In practice, you wouldn't bother with a Reader if you're doing all of this inline and just want to parse json. The Reader can be useful though in concert with something like a library for making HTTP requests and returning the result as an object @see https://github.com/beatthat/requests

TODO: change the example below to something that motivates why you would use a Reader, e.g. internal code from Requests

```c#
using BeatThat.Serializers.Newtonsoft;

public class MyClass
{
    [Serializable]
    public struct Data
    {
        public string name;
        public string type;
    }

    public const string JSON =
@"{
""name"":""my name"",
""type"":""my type"",
}";

    public Data ReadItem()
    {
      var reader = new JsonNetSerializer<Data>(); // in real apps, usually share a static instance
      using (var s = new MemoryStream(Encoding.UTF8.GetBytes(JSON)))
      {
          try
          {
              return reader.ReadOne(s);
          }
          catch (Exception e)
          {
              Debug.LogWarning("invalid json: " + e.Message);
          }
      }
    }

}
```

#### Change the underlying json lib to Newtonsoft (or other)

You can change the json impl to newtonsoft as follows:

- add the newtonsoft [JsonNet](https://www.newtonsoft.com/json) dll to `Assets/Plugins` (NOTE: as of this writing Unity only works with the 2.0 version and you will need to make some link.xml entries if using iOS)
- Install via terminal `cd your-unity-project-root && npm install --save beatthat/serializers-newtonsoft`
- put the code snippet below somewhere early in execution of your game

```c#
BeatThat.Serializers.Newtonsoft.JsonNetSerializer.UseAsDefault();
```

## Development

You can edit the code and samples in the test environment and then use `npm run test-install` to sync changes back to the package src.

```
    npm run test-install
    cd test

    # edit code under Assets/Plugins/packages/beatthat/serializers-newtonsoft
    # edit samples under Assets/Samples/packages/beatthat/serializers-newtonsoft

    # sync changes back to src
    npm run test-overwrite2src
```

**REMEMBER:** changes made under the test folder are not saved to the package
unless they are copied back into the source folder
