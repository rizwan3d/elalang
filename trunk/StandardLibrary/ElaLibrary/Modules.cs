using Ela.Linking;
using Ela.Library.General;
//using Ela.Library.Collections;

[assembly: ElaModule("$Core", typeof(CoreModule))]

[assembly: ElaModule("Debug", typeof(DebugModule))]
[assembly: ElaModule("String", typeof(StringModule))]
[assembly: ElaModule("$StringBuilder", typeof(StringBuilderModule))]
[assembly: ElaModule("Char", typeof(CharModule))]
[assembly: ElaModule("$Guid", typeof(GuidModule))]
[assembly: ElaModule("Real", typeof(RealModule))]
[assembly: ElaModule("$DateTime", typeof(DateTimeModule))]
[assembly: ElaModule("Con", typeof(ConModule))]
[assembly: ElaModule("Shell", typeof(ShellModule))]
[assembly: ElaModule("$Async", typeof(AsyncModule))]

//[assembly: ElaModule("MutableMap", typeof(MutableMapModule))]
//[assembly: ElaModule("Map", typeof(MapModule))]
//[assembly: ElaModule("Set", typeof(SetModule))]
//[assembly: ElaModule("Array", typeof(ArrayModule))]
//[assembly: ElaModule("Queue", typeof(QueueModule))]