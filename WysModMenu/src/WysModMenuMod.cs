using GmmlPatcher;

using UndertaleModLib.Models;

using WysApi.Api;

namespace WysModMenu;

// ReSharper disable once ClassNeverInstantiated.Global
public class WysModMenuMod : IGameMakerMod {
    private static readonly Dictionary<string, string?> configMenus = new();

    public void Load(int audioGroup, ModData currentMod) { }

    public void LateLoad(int audioGroup, ModData currentMod) {
        if(audioGroup != 0) return;

        UndertaleGameObject modsMenu = Menus.CreateMenu("Mods", Patcher.mods.Select(mod => {
            bool hasDescription = string.IsNullOrWhiteSpace(mod.metadata.description);
            string authors = string.Join(", ", mod.metadata.authors);
            return new Menus.WysMenuOption($"\"{mod.metadata.name} v{mod.metadata.version}\"") {
                instance = configMenus.TryGetValue(mod.metadata.id, out string? menu) ? menu : null,
                tooltipScript = Menus.Vanilla.Tooltips.Text,
                tooltipArgument = new UndertaleString(hasDescription ? $"by {authors}" :
                $"{mod.metadata.description}\n\n by {authors}").ToString()
            };
        }).ToArray());

        Menus.InsertMenuOptionFromEnd(Menus.Vanilla.Settings, 1, new Menus.WysMenuOption("\"Mods\"") {
            instance = modsMenu.Name.Content
        });
    }

    public static void SetConfigMenu(string id, string? menu) => configMenus[id] = menu;
}
