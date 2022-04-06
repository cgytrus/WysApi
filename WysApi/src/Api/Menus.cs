using System.Globalization;
using System.Text;

using GmmlHooker;

using GmmlPatcher;

using UndertaleModLib;
using UndertaleModLib.Models;

namespace WysApi.Api;
// ReSharper disable MemberCanBePrivate.Global MemberCanBeInternal UnusedMember.Global
// ReSharper disable UnusedMethodReturnValue.Global OutParameterValueIsAlwaysDiscarded.Global

public static class Menus {
    public static class Vanilla {
        public const string Prefix = "obj_menu_";

        public static class Tooltips {
            // same as scr_mirror_text_input... lol
            public const string Text = "gml_Script_scr_return_input";
            public const string OnOff = "gml_Script_scr_return_onofftext";
            public const string Percentage = "gml_Script_scr_return_percentage";
        }

        public const string Main = $"{Prefix}MAIN";
        public const string Difficulty = $"{Prefix}Difficuty";
        public const string Settings = $"{Prefix}Settings";
        public const string UnicornPower = $"{Prefix}UnicornPower";
        public const string Exit = $"{Prefix}Exit";

        public const string Sound = $"{Prefix}Sound";
        public const string Graphics = $"{Prefix}Graphics";
        public const string Gameplay = $"{Prefix}Gameplay";
        public const string Language = $"{Prefix}Language";
        public const string More = $"{Prefix}More";

        public const string SquidVisuals = $"{Prefix}SquidVisuals";
        public const string AdvancedGraphics = $"{Prefix}GraphicsAdv";

        public const string Speedrun = $"{Prefix}Speedrun";
        public const string Hacks = $"{Prefix}Hacks";
        public const string Extras = $"{Prefix}Extras";
    }

    public const string MenuPrefix = $"{Vanilla.Prefix}wysapi_";
    public const string ScriptPrefix = "scr_wysapi_";

    public sealed record WysMenuSettings(
        bool executeScriptsOnSwitch = false,
        bool executeScriptsOnConfirm = true,
        bool executeScriptsOnExit = false,
        bool exitSubmenuAfterConfirm = false,
        bool allowLoopingUpDown = true,
        string? executeScriptOnStart = null,
        bool enableUiSounds = true) {
        public string GetCode() =>
            $@"bExecuteScriptsOnSwitch = {executeScriptsOnSwitch.ToString().ToLowerInvariant()}
bExecuteScriptsOnConfirm = {executeScriptsOnConfirm.ToString().ToLowerInvariant()}
bExecuteScriptsOnExit = {executeScriptsOnExit.ToString().ToLowerInvariant()}
bExitSumbenuAfterConfirm = {exitSubmenuAfterConfirm.ToString().ToLowerInvariant()}
bAllowLoopingUpDown = {allowLoopingUpDown.ToString().ToLowerInvariant()}
scrExecuteScriptOnStart = {executeScriptOnStart ?? "-1"}
bEnableUISounds = {enableUiSounds.ToString().ToLowerInvariant()}";
    }

    public sealed record WysMenuOption(
        string name,
        string? instance = null,
        string? script = null,
        string? scriptArgument = null,
        string? tooltipScript = null,
        string? tooltipArgument = null) {

        // shush i know this code sucks idk how to make it better
        public string GetAddCode() => GetCode(false);
        public string GetInsertCode(int index) => GetCode(true, false, index);
        public string GetInsertFromEndCode(int index) => GetCode(true, true, index);

        private string GetCode(bool insert, bool fromEnd = false, int index = 0) =>
            $@"{GetLine(insert, "liMenuItemNames", name, fromEnd, index)}
{GetLine(insert, "liMenuItemInstances", instance ?? "-1", fromEnd, index)}
{GetLine(insert, "liMenuItemScripts", script is null ? "-1" : $"{script}", fromEnd, index)}
{GetLine(insert, "liMenuItemScriptArguments", scriptArgument ?? "-1", fromEnd, index)}
{GetLine(insert, "liMenuItemTooltipScript", tooltipScript is null ? "-1" : $"{tooltipScript}", fromEnd, index)}
{GetLine(insert, "liMenuItemTooltipArgument", tooltipArgument ?? "-1", fromEnd, index)}";

        private static string GetLine(bool insert, string list, string value, bool fromEnd = false, int index = 0) =>
            insert ? $"ds_list_insert({list}, {(fromEnd ? $"ds_list_size({list}) - {index}" : $"{index}")}, {value})" :
            $"ds_list_add({list}, {value})";
    }

    public static string GetMenuScriptName(string menu) => $"gml_Object_{menu}_Other_10";

    public static void AddMenuOption(string menu, WysMenuOption option) => AddMenuOption(Patcher.data, menu, option);
    public static void AddMenuOption(UndertaleData data, string menu, WysMenuOption option) =>
        Hooker.HookCode(data, GetMenuScriptName(menu), $"#orig#()\n{option.GetAddCode()}");

    public static void InsertMenuOption(string menu, int index, WysMenuOption option) =>
        InsertMenuOption(Patcher.data, menu, index, option);
    public static void InsertMenuOption(UndertaleData data, string menu, int index, WysMenuOption option) =>
        Hooker.HookCode(data, GetMenuScriptName(menu), $"#orig#()\n{option.GetInsertCode(index)}");

    public static void InsertMenuOptionFromEnd(string menu, int index, WysMenuOption option) =>
        InsertMenuOptionFromEnd(Patcher.data, menu, index, option);
    public static void InsertMenuOptionFromEnd(UndertaleData data, string menu, int index, WysMenuOption option) =>
        Hooker.HookCode(data, GetMenuScriptName(menu), $"#orig#()\n{option.GetInsertFromEndCode(index)}");

    public static UndertaleGameObject CreateMenu(string menu, params WysMenuOption[] options) =>
        CreateMenu(Patcher.data, menu, options);
    public static UndertaleGameObject CreateMenu(UndertaleData data, string name, params WysMenuOption[] options) =>
        CreateMenu(data, name, new WysMenuSettings(), options);

    public static UndertaleGameObject CreateMenu(string menu, WysMenuSettings settings,
        params WysMenuOption[] options) => CreateMenu(Patcher.data, menu, settings, options);
    public static UndertaleGameObject CreateMenu(UndertaleData data, string name, WysMenuSettings settings,
        params WysMenuOption[] options) {
        UndertaleGameObject menu = new() {
            Name = data.Strings.MakeString($"{MenuPrefix}{name}"),
            ParentId = data.GameObjects.ByName($"{Vanilla.Prefix}instance")
        };
        data.GameObjects.Add(menu);

        StringBuilder codeBuilder = new();

        codeBuilder.AppendLine(settings.GetCode());
        foreach(WysMenuOption option in options)
            codeBuilder.AppendLine(option.GetAddCode());

        Hooker.ReplaceGmlSafe(menu.EventHandlerFor(EventType.Other, EventSubtypeOther.User0, data),
            codeBuilder.ToString(), data);
        return menu;
    }

    public static string CreateToggleMenu(string menu, string name, string setCode, string preselectCode,
        string? tooltipScript = null, string? tooltipArgument = null) =>
        CreateToggleMenu(Patcher.data, name, setCode, preselectCode, tooltipScript, tooltipArgument);
    public static string CreateToggleMenu(UndertaleData data, string name, string setCode, string preselectCode,
        string? tooltipScript = null, string? tooltipArgument = null) {
        UndertaleScript setScript =
            Hooker.CreateSimpleScript(data, $"{ScriptPrefix}set_{name}", setCode, 1);
        UndertaleScript preselectScript =
            Hooker.CreateSimpleScript(data, $"{ScriptPrefix}preselect_{name}", preselectCode, 0);

        UndertaleGameObject menu = CreateMenu(data, $"toggle_{name}", new WysMenuSettings {
            executeScriptsOnSwitch = true,
            exitSubmenuAfterConfirm = true,
            executeScriptsOnConfirm = false,
            executeScriptOnStart = preselectScript.Name.Content
        }, new WysMenuOption("loca_text(\"menu_off\")") {
            script = setScript.Name.Content,
            scriptArgument = "false",
            tooltipScript = tooltipScript,
            tooltipArgument = tooltipArgument
        }, new WysMenuOption("loca_text(\"menu_on\")") {
            script = setScript.Name.Content,
            scriptArgument = "true",
            tooltipScript = tooltipScript,
            tooltipArgument = tooltipArgument
        });

        return menu.Name.Content;
    }

    public static WysMenuOption CreateToggleOption(string name, string internalName,
        string setCode, string preselectCode, string variable,
        string? tooltipScript = null, string? tooltipArgument = null) =>
        CreateToggleOption(Patcher.data, name, internalName, setCode, preselectCode, variable, tooltipScript,
            tooltipArgument);
    public static WysMenuOption CreateToggleOption(UndertaleData data, string name, string internalName,
        string setCode, string preselectCode, string variable,
        string? tooltipScript = null, string? tooltipArgument = null) {
        string menuName = CreateToggleMenu(data, internalName, setCode, preselectCode, tooltipScript, tooltipArgument);

        return new WysMenuOption(name) {
            instance = menuName,
            tooltipScript = Vanilla.Tooltips.OnOff,
            tooltipArgument = variable
        };
    }

    public static (string menuName, string returnName) CreateChangeMenu(string menu, string name,
        string changeCode, string returnCode, double step, bool enableUiSounds = true) =>
        CreateChangeMenu(Patcher.data, name, changeCode, returnCode, step, enableUiSounds);
    public static (string menuName, string returnName) CreateChangeMenu(UndertaleData data, string name,
        string changeCode, string returnCode, double step, bool enableUiSounds = true) {
        UndertaleScript changeScript = Hooker.CreateSimpleScript(data, $"{ScriptPrefix}change_{name}", changeCode, 1);
        UndertaleScript returnScript = Hooker.CreateSimpleScript(data, $"{ScriptPrefix}return_{name}", returnCode, 0);

        UndertaleGameObject menu = CreateMenu(data, $"change_{name}", new WysMenuSettings {
            allowLoopingUpDown = false,
            executeScriptsOnSwitch = true,
            enableUiSounds = enableUiSounds
        }, new WysMenuOption("\"[+]\"") {
            script = changeScript.Name.Content,
            scriptArgument = step.ToString(CultureInfo.InvariantCulture),
            tooltipScript = returnScript.Name.Content
        }, new WysMenuOption("\"[-]\"") {
            script = changeScript.Name.Content,
            scriptArgument = (-step).ToString(CultureInfo.InvariantCulture),
            tooltipScript = returnScript.Name.Content
        });

        return (menu.Name.Content, returnScript.Name.Content);
    }

    public static WysMenuOption CreateChangeOption(string name, string internalName,
        string changeCode, string returnCode, double step) =>
        CreateChangeOption(Patcher.data, name, internalName, changeCode, returnCode, step);
    public static WysMenuOption CreateChangeOption(UndertaleData data, string name, string internalName,
        string changeCode, string returnCode, double step) {
        (string menuName, string returnName) = CreateChangeMenu(data, internalName, changeCode, returnCode, step);

        return new WysMenuOption(name) {
            instance = menuName,
            tooltipScript = returnName
        };
    }
}
