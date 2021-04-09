using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventoryApp.Components
{
    public partial class PageLink
    {
        [Parameter]
        public int CurrentPage { get; set; }

        [Parameter]
        public int TotalPage { get; set; }

        [Parameter]
        public int Radius { get; set; } = 3;
        [Parameter]
        public EventCallback<int> SelectedPage { get; set; }

        private List<LinkModel> links;
        protected override void OnParametersSet()
        {
            LoadPage();
        }

        private async Task SelectedPageInternal(LinkModel link)
        {
            if (link.Page == CurrentPage)
            {
                CurrentPage = link.Page;
                await SelectedPage.InvokeAsync(link.Page);
                return;
            }

            if (link.Enable)
            {
                CurrentPage = link.Page;
                await SelectedPage.InvokeAsync(link.Page);
                return;
            }
        }

        private void LoadPage()
        {
            links = new List<LinkModel>();
            var isPreviousPageLinkEnable = CurrentPage != 1;
            var previousPage = CurrentPage - 1;
            links.Add(new LinkModel(previousPage, isPreviousPageLinkEnable, "Previous"));

            for (int i = 1; i <= TotalPage; i++)
            {
                if (i >= CurrentPage - Radius && i <= CurrentPage + Radius)
                {
                    links.Add(new LinkModel(i)
                    { Active = CurrentPage == i });
                }
            }

            var isNextPageLinkEnable = CurrentPage != TotalPage;
            var nextPage = CurrentPage + 1;
            links.Add(new LinkModel(nextPage, isNextPageLinkEnable, "Next"));
        }

        class LinkModel
        {
            public LinkModel(int page) : this(page, true)
            {
            }

            public LinkModel(int page, bool enable) : this(page, enable, page.ToString())
            {
            }

            public LinkModel(int page, bool enabel, string text)
            {
                Page = page;
                Enable = enabel;
                Text = text;
            }

            public int Page { get; set; }

            public bool Enable { get; set; } = true;
            public string Text { get; set; }

            public bool Active { get; set; }
        }
    }
}