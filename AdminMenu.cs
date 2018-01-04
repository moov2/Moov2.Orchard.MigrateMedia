using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Navigation;

namespace Moov2.Orchard.MigrateMedia
{
    public class AdminMenu : INavigationProvider
    {
        public Localizer T { get; set; }

        public string MenuName
        {
            get { return "admin"; }
        }

        public void GetNavigation(NavigationBuilder builder)
        {
            builder.Add(T("Media"), "6", item => item.Add(T("Migrate"), "6", i => i.Action("Index", "Admin", new { area = "Moov2.Orchard.MigrateMedia" }).Permission(StandardPermissions.SiteOwner)));
        }
    }
}
