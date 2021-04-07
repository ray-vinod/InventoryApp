using System.Collections.Generic;

namespace InventoryApp.Shared
{
    public partial class NavMenu
    {
        public bool collapseNavMenu = true;
        public List<MyMenu> subMenu;

        public string NavMenuCssClass => collapseNavMenu ? "collapse" : null;


        public void ToggleNavMenu()
        {
            collapseNavMenu = !collapseNavMenu;
        }

        protected override void OnInitialized()
        {
            subMenu = new List<MyMenu>
            {
                new MyMenu { Name = "product" },
                new MyMenu { Name = "receive" },
                new MyMenu { Name = "issue" },
                new MyMenu { Name = "stock" },
                new MyMenu { Name = "report" },
                new MyMenu { Name = "dashboard" },
                new MyMenu { Name = "email" },
                new MyMenu { Name = "prefix" },
                new MyMenu { Name = "task" }
            };

        }

        public void ToggleSubMenu(string menuName)
        {
            foreach (var menu in subMenu)
            {
                if (menu.Name == menuName)
                {
                    menu.Status = !menu.Status;
                }
                else
                {
                    menu.Status = false;
                }
            }
        }

        public void SetFalse()
        {
            foreach (var menu in subMenu)
            {
                menu.Status = false;
            }
        }

        public class MyMenu
        {
            public string Name { get; set; }
            public bool Status { get; set; } = false;
        }

    }
}