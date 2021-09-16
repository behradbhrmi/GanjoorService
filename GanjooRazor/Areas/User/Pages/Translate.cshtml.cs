﻿using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using GanjooRazor.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RMuseum.Models.Ganjoor;
using RMuseum.Models.Ganjoor.ViewModels;

namespace GanjooRazor.Areas.User.Pages
{
    [IgnoreAntiforgeryToken(Order = 1001)]
    public class TranslateModel : PageModel
    {
        /// <summary>
        /// page
        /// </summary>
        public GanjoorPageCompleteViewModel PageInformation { get; set; }

        /// <summary>
        /// translations
        /// </summary>
        public GanjoorLanguage[] Languages { get; set; }

        /// <summary>
        /// translation
        /// </summary>
        public GanjoorPoemTranslationViewModel Translation { get; set; }

        /// <summary>
        /// fatal error
        /// </summary>
        public string FatalError { get; set; }

        public string GetVersePosition(GanjoorVerseViewModel verse)
        {
            switch (verse.VersePosition)
            {
                case VersePosition.Right:
                    return "مصرع اول";
                case VersePosition.Left:
                    return "مصرع دوم";
                case VersePosition.CenteredVerse1:
                    return "مصرع اول بند";
                case VersePosition.CenteredVerse2:
                    return "مصرع دوم بند";
                case VersePosition.Paragraph:
                    return "پاراگراف نثر";
                case VersePosition.Single:
                    return "نیمایی یا آزاد";
            }
            return "نامعتبر";
        }

        public int GetVerseCoupletNumber(GanjoorVerseViewModel verse)
        {
            int n = 1;
            VersePosition pre = VersePosition.Right;
            foreach (var v in PageInformation.Poem.Verses)
            {
                if (v.Id == verse.Id)
                {
                    if (pre == VersePosition.CenteredVerse1 && v.VersePosition != VersePosition.CenteredVerse2)
                        n++;
                    return n;
                }
                if (v.VersePosition == VersePosition.Left || v.VersePosition == VersePosition.CenteredVerse2
                    || v.VersePosition == VersePosition.Single || v.VersePosition == VersePosition.Paragraph)
                    n++;
                else
                if (pre == VersePosition.CenteredVerse1)
                    n++;
                pre = v.VersePosition;
            }
            return -1;
        }

        /// <summary>
        /// get
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnGetAsync()
        {
            FatalError = "";
            using (HttpClient secureClient = new HttpClient())
            {
                if (await GanjoorSessionChecker.PrepareClient(secureClient, Request, Response))
                {
                    HttpResponseMessage response = await secureClient.GetAsync($"{APIRoot.Url}/api/translations/languages");
                    if (!response.IsSuccessStatusCode)
                    {
                        FatalError = JsonConvert.DeserializeObject<string>(await response.Content.ReadAsStringAsync());
                        return Page();
                    }
                    response.EnsureSuccessStatusCode();

                    Languages = JsonConvert.DeserializeObject<GanjoorLanguage[]>(await response.Content.ReadAsStringAsync());

                    if(Languages.Length == 0)
                    {
                        FatalError = "<a role=\"button\" target=\"_blank\" href=\"/User/Languages\" class=\"actionlink\">معرفی زبان‌ها و نویسش‌ها</a>";
                        return Page();
                    }
                    

                    var pageUrlResponse = await secureClient.GetAsync($"{APIRoot.Url}/api/ganjoor/pageurl?id={Request.Query["id"]}");
                    pageUrlResponse.EnsureSuccessStatusCode();
                    var pageUrl = JsonConvert.DeserializeObject<string>(await pageUrlResponse.Content.ReadAsStringAsync());

                    var pageQuery = await secureClient.GetAsync($"{APIRoot.Url}/api/ganjoor/page?url={pageUrl}");
                    pageQuery.EnsureSuccessStatusCode();
                    PageInformation = JObject.Parse(await pageQuery.Content.ReadAsStringAsync()).ToObject<GanjoorPageCompleteViewModel>();


                    Translation = new GanjoorPoemTranslationViewModel()
                    {
                        Language = Languages[0],
                        PoemId = PageInformation.Id,
                        Title = "",
                        Published = false,
                        TranslatedVerses = PageInformation.Poem.Verses.Select(v =>
                        new GanjoorVerseTranslationViewModel()
                        {
                            Verse = v,
                            TText = ""
                        }
                        ).ToArray()
                    };
                }
                else
                {
                    FatalError = "لطفا از گنجور خارج و مجددا به آن وارد شوید.";
                }
            }
            return Page();
        }

        public async Task<IActionResult> OnPostDeletePoemCorrectionsAsync(int poemid)
        {
            using (HttpClient secureClient = new HttpClient())
            {
                if (await GanjoorSessionChecker.PrepareClient(secureClient, Request, Response))
                {
                    HttpResponseMessage response = await secureClient.DeleteAsync(
                        $"{APIRoot.Url}/api/ganjoor/poem/correction/{poemid}");
                    response.EnsureSuccessStatusCode();
                    return new OkObjectResult(true);
                }
            }
            return new BadRequestObjectResult("لطفا از گنجور خارج و مجددا به آن وارد شوید.");
        }

        public async Task<IActionResult> OnPostSendPoemCorrectionsAsync(int poemid, string[] verseOrderText, string rhythm, string note)
        {
            using (HttpClient secureClient = new HttpClient())
            {
                if (await GanjoorSessionChecker.PrepareClient(secureClient, Request, Response))
                {
                    string title = null;
                    List<GanjoorVerseVOrderText> vOrderTexts = new List<GanjoorVerseVOrderText>();
                    foreach (string v in verseOrderText)
                    {
                        var vParts = v.Split("TextSeparator", System.StringSplitOptions.RemoveEmptyEntries);
                        int vOrder = int.Parse(vParts[0]);
                        if (vOrder == 0)
                            title = vParts[1];
                        else
                        {
                            vOrderTexts.Add
                                (
                                new GanjoorVerseVOrderText()
                                {
                                    VORder = vOrder,
                                    Text = vParts[1]
                                }
                                );
                        }
                    }

                    if (title == null && vOrderTexts.Count == 0 && rhythm == null)
                        return new BadRequestObjectResult("شما هیچ تغییری در متن نداده‌اید!");

                    GanjoorPoemCorrectionViewModel correction = new GanjoorPoemCorrectionViewModel()
                    {
                        PoemId = poemid,
                        Title = title,
                        VerseOrderText = vOrderTexts.ToArray(),
                        Rhythm = rhythm,
                        Note = note
                    };

                    HttpResponseMessage response = await secureClient.PostAsync(
                        $"{APIRoot.Url}/api/ganjoor/poem/correction",
                        new StringContent(JsonConvert.SerializeObject(correction),
                        Encoding.UTF8,
                        "application/json"));
                    if (!response.IsSuccessStatusCode)
                    {
                        return new BadRequestObjectResult(JsonConvert.DeserializeObject<string>(await response.Content.ReadAsStringAsync()));
                    }
                    return new OkObjectResult(true);
                }
                else
                {
                    return new BadRequestObjectResult("لطفا از گنجور خارج و مجددا به آن وارد شوید.");
                }
            }
        }
    }
}
