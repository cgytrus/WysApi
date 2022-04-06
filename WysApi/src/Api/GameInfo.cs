using GmmlPatcher;

using UndertaleModLib;
using UndertaleModLib.Models;

namespace WysApi.Api;
// ReSharper disable MemberCanBePrivate.Global MemberCanBeInternal UnusedMember.Global
// ReSharper disable UnusedMethodReturnValue.Global OutParameterValueIsAlwaysDiscarded.Global

public static class GameInfo {
    private static UndertaleString? _cachedMutableGameVersion;
    private static string? _cachedGameVersion;

    public static string GetGameVersion() => GetGameVersion(Patcher.data);
    public static string GetGameVersion(UndertaleData data) =>
        _cachedGameVersion ?? GetMutableGameVersion(data).Content;

    public static UndertaleString GetMutableGameVersion() => GetMutableGameVersion(Patcher.data);
    public static UndertaleString GetMutableGameVersion(UndertaleData data) {
        _cachedMutableGameVersion ??= ((UndertaleResourceById<UndertaleString?, UndertaleChunkSTRG>?)data.Code
            .ByName("gml_Object_obj_menu_manager_Draw_0").Instructions.FirstOrDefault(code =>
                code.Kind == UndertaleInstruction.Opcode.Push && code.Type1 == UndertaleInstruction.DataType.String)?
            .Value)?.Resource ?? data.Strings.MakeString("0.0");
        if(_cachedGameVersion is not null)
            return _cachedMutableGameVersion;

        _cachedGameVersion = _cachedMutableGameVersion.Content;
        return _cachedMutableGameVersion;
    }
}
