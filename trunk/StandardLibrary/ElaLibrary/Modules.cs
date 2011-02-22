using Ela.Linking;
using Ela.StandardLibrary;

[assembly: ElaModule("CoreInternal", typeof(CoreInternalModule))]

[assembly: ElaModule("Debug", typeof(DebugModule))]
[assembly: ElaModule("String", typeof(StringModule))]
[assembly: ElaModule("StringBuilder", typeof(StringBuilderModule))]
[assembly: ElaModule("Char", typeof(CharModule))]
[assembly: ElaModule("Guid", typeof(GuidModule))]
[assembly: ElaModule("Real", typeof(RealModule))]
[assembly: ElaModule("Con", typeof(ConModule))]