﻿using Ela.Linking;
using Ela.Library.General;
using Ela.Library.Collections;

[assembly: ElaModule("CoreInternal", typeof(CoreInternalModule))]

[assembly: ElaModule("Debug", typeof(DebugModule))]
[assembly: ElaModule("String", typeof(StringModule))]
[assembly: ElaModule("StringBuilder", typeof(StringBuilderModule))]
[assembly: ElaModule("Char", typeof(CharModule))]
[assembly: ElaModule("Guid", typeof(GuidModule))]
[assembly: ElaModule("Real", typeof(RealModule))]
[assembly: ElaModule("DateTime", typeof(DateTimeModule))]
[assembly: ElaModule("Con", typeof(ConModule))]
[assembly: ElaModule("Shell", typeof(ShellModule))]

[assembly: ElaModule("MutableMap", typeof(MutableMapModule))]
[assembly: ElaModule("Map", typeof(MapModule))]
[assembly: ElaModule("Set", typeof(SetModule))]
[assembly: ElaModule("Array", typeof(ArrayModule))]