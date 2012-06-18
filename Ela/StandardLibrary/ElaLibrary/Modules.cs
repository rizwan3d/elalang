using Ela.Linking;
using Ela.Library.General;
//using Ela.Library.Collections;

[assembly: ElaModule("$Con", typeof(ConModule))]
[assembly: ElaModule("$Core", typeof(CoreModule))]
[assembly: ElaModule("$Format", typeof(FormatModule))]
[assembly: ElaModule("$Cell", typeof(CellModule))]
[assembly: ElaModule("$IO", typeof(IOModule))]
[assembly: ElaModule("$Char", typeof(CharModule))]
[assembly: ElaModule("$String", typeof(StringModule))]
[assembly: ElaModule("$Number", typeof(NumberModule))]
[assembly: ElaModule("$DateTime", typeof(DateTimeModule))]
[assembly: ElaModule("$Record", typeof(RecordModule))]
[assembly: ElaModule("$Reflect", typeof(ReflectModule))]

[assembly: ElaModule("experimental", typeof(Experimental))]
[assembly: ElaModule("debug", typeof(DebugModule))]
//[assembly: ElaModule("StringBuilder", typeof(StringBuilderModule))]
//[assembly: ElaModule("Guid", typeof(GuidModule))]
//[assembly: ElaModule("Shell", typeof(ShellModule))]
//[assembly: ElaModule("Async", typeof(AsyncModule))]

//[assembly: ElaModule("MutableMap", typeof(MutableMapModule))]
//[assembly: ElaModule("Map", typeof(MapModule))]
//[assembly: ElaModule("Set", typeof(SetModule))]
//[assembly: ElaModule("Array", typeof(ArrayModule))]
//[assembly: ElaModule("Queue", typeof(QueueModule))]
