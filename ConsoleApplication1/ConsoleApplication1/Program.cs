using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Configuration;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.XPath;


namespace ConsoleApplication1
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                Console.Write("start\n");
                var x = new Tests();
                x.Test().Wait();
                Console.Write("\n");
                Console.Write("end\n");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("=============================================================");
            }
        }
    }
    class Tests{
        public async Task Test()
        {
            try
            {
                // get videoId 
                var url = "https://www.youtube.com/watch?v=gKFavzPexO4";
                var text = await Test2(url);
                var a1 = Regex.Match(text, "window\\[\"ytInitialData\"\\]\\s*=\\s*(.+?})(?:\"\\))?;", RegexOptions.Singleline);
                dynamic document = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<String, dynamic>>(a1.Groups[1].Value);
                var videoId = document["currentVideoEndpoint"]["watchEndpoint"]["videoId"];
                Console.WriteLine(videoId);

                //////////////
                // // var b1 = Regex.Match(text, "window\\[\"ytInitialData\"\\] = ({.+});\\s*</script>", RegexOptions.Singleline);
                // var b1 = Regex.Match(text, "window\\[\"ytInitialData\"\\] = (\\{.+\\});.*window\\[\"ytInitialPlayerResponse\"\\]", RegexOptions.Singleline);
                // Console.WriteLine(b1.Groups[1].Value);
                // dynamic b2 = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<String, dynamic>>(b1.Groups[1].Value);
                // // Console.WriteLine(b2);
                // var b3 = b2["contents"]["liveChatRenderer"]["continuations"][0]["timedContinuationData"]["continuation"].ToString();
                // var b4 = "https://www.youtube.com/live_chat_replay?continuation=" + System.Web.HttpUtility.UrlEncode(b3);
                // var b5 = await Test2(b4);
                // // Console.WriteLine(b5);
                // return;
                //////////////
                
                // get continuation
                var url2 = "https://www.youtube.com/live_chat?v=" + videoId + "&is_popout=1";
                var text2 = await Test2(url2);
                var a2 = Regex.Match(text2, "window\\[\"ytInitialData\"\\] = ({.+});\\s*</script>", RegexOptions.Singleline);
                dynamic document2 = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<String, dynamic>>(a2.Groups[1].Value);
                var continuation = document2["contents"]["liveChatRenderer"]["continuations"][0]["timedContinuationData"]["continuation"].ToString();
                Console.WriteLine(continuation);

                //
                for (var i = 0; i < 10000; i++)
                {
                    await Task.Delay(100);
                    var url3 = "https://www.youtube.com/live_chat/get_live_chat?pbj=1&continuation=" + System.Web.HttpUtility.UrlEncode(continuation);
                    var text3 = await Test2(url3);
                    Console.WriteLine(i);
                    continuation = Test3(text3, continuation);
                }

                Console.WriteLine("End");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
            }
        }

        private String Test3(string text3, string continuation)
        {
            try
            {
                dynamic document3 = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<String, dynamic>>(text3);
                continuation = document3["response"]["continuationContents"]["liveChatContinuation"]["continuations"][0]["timedContinuationData"]["continuation"];
                var x = document3["response"]["continuationContents"]["liveChatContinuation"];
                if (x.ContainsKey("actions"))
                {
                    foreach (var y in x.actions)
                    {
                        if (y.ContainsKey("addChatItemAction"))
                        {
                            var item = y.addChatItemAction.item;
                            var mess = item;
                            // 通常コメント
                            if(item.ContainsKey("liveChatTextMessageRenderer"))
                            {
                                mess = item.liveChatTextMessageRenderer;
                                var authorName = mess.authorName.simpleText;
                                foreach (var r in mess.message.runs)
                                {
                                    if (r.ContainsKey("text"))
                                    {
                                        Console.WriteLine("[" + authorName + "]" + r.text);    
                                    }
                                    else if (r.ContainsKey("emoji"))
                                    {
                                        Console.WriteLine(r.emoji.image.thumbnails[0].url); 
                                        Console.WriteLine("[" + authorName + "]" + r.emoji.image.accessibility.accessibilityData.label);    
                                    }
                                }
                            }
                            // スパチャ
                            else if (item.ContainsKey("liveChatPaidMessageRenderer"))
                            {
                                Console.WriteLine("スパチャ");
                                mess = item.liveChatPaidMessageRenderer;
                                var authorName = mess.authorName.simpleText;
                                var cost = mess.purchaseAmountText.simpleText;
                                Console.WriteLine("[" + authorName + "]" + cost);
                                foreach(var r in mess.message.runs)
                                {
                                    if (r.ContainsKey("text"))
                                    {
                                        Console.WriteLine("[" + authorName + "]" + r.text);    
                                    }
                                    else if (r.ContainsKey("emoji"))
                                    {
                                        Console.WriteLine(r.emoji.image.thumbnails[0].url); 
                                        Console.WriteLine("[" + authorName + "]" + r.emoji.image.accessibility.accessibilityData.label);    
                                    }
                                }
                            }
                            // スパステ
                            else if(item.ContainsKey("liveChatPaidStickerRenderer"))
                            {
                                Console.WriteLine("スパステ");
                                mess = item.liveChatPaidStickerRenderer;
                                var authorName = mess.authorName.simpleText;
                                var cost = mess.purchaseAmountText.simpleText;
                                Console.WriteLine("[" + authorName + "]" + cost);
                            }
                            // ???
                            else if(item.ContainsKey("liveChatTickerPaidStickerItemRenderer"))
                            {
                                Console.WriteLine("●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●");
                                Console.WriteLine("liveChatTickerPaidStickerItemRenderer");
                                Console.WriteLine(item.liveChatTickerPaidStickerItemRenderer);
                            }
                            // ???
                            else if(item.ContainsKey("liveChatTickerPaidMessageItemRenderer"))
                            {
                                Console.WriteLine("●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●");
                                Console.WriteLine("liveChatTickerPaidMessageItemRenderer");
                                Console.WriteLine(item.liveChatTickerPaidMessageItemRenderer);
                            }
                            // メンバーシップ登録
                            else if(item.ContainsKey("liveChatMembershipItemRenderer"))
                            {
                                Console.WriteLine("●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●");
                                Console.WriteLine("liveChatMembershipItemRenderer");
                                mess = item.liveChatMembershipItemRenderer;
                                var authorName = mess.authorName.simpleText;
                                var welcomeText = "";
                                foreach (var r in mess.headerSubtext.runs)
                                {
                                    welcomeText += r.text;
                                }
                                Console.WriteLine("[" + authorName + "]" + welcomeText);
                                
                            }
                            // ???
                            else if(item.ContainsKey("liveChatTickerSponsorItemRenderer"))
                            {
                                Console.WriteLine("●●●●●●●●●●●●●●●●●●●●●●●●●●●●●●");
                                Console.WriteLine("liveChatTickerSponsorItemRenderer");
                                Console.WriteLine(item.liveChatTickerSponsorItemRenderer);
                            }
                            else
                            {
                                Console.WriteLine("???????????????????????????????????");
                                Console.WriteLine(item);
                                continue;
                            }
                            // メンバーシップ情報
                            if(mess.ContainsKey("authorBadges"))
                            {
                                var memberThumbnail = mess.authorBadges[0].liveChatAuthorBadgeRenderer.customThumbnail.thumbnails[0].url;
                                var memberLabel = mess.authorBadges[0].liveChatAuthorBadgeRenderer.accessibility.accessibilityData.label;
                                Console.WriteLine("★★★★★" + memberLabel); 
                                Console.WriteLine(memberThumbnail); 
                            }
                            // メンバーシップ登録？？？
                        }
                    }
                }
                Console.WriteLine("---------------------");
                return continuation;
            }
            catch (Exception e)
            {
                dynamic document3 = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<String, dynamic>>(text3);
                var x = document3["response"]["continuationContents"]["liveChatContinuation"];
                Console.WriteLine(e.Message);
                Console.WriteLine(x.actions);
                Console.WriteLine("+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
                return continuation;
            }
        }
        
        private async Task<string> Test2(string url)
        {
            try
            {
                var client = new HttpClient();
                var userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:59.0) Gecko/20100101 Firefox/59.0";
                client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", userAgent);
                var result = await client.GetByteArrayAsync(url);
                return Encoding.UTF8.GetString(result);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("##########################################################################");
                return null;
            }
        }
    }
}