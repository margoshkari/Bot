using Google.Cloud.Vision.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Bot
{
    public class TelegramBot
    {
        static string info;
        static ITelegramBotClient bot = new TelegramBotClient("5326298021:AAF_P93aGZSAM3LHRZoQSFa0JzU0itqL7tM");
        private static bool isStarted = false;
        private static bool isText = false;
        private static bool isFace = false;

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Некоторые действия
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                var message = update.Message;
                if (isStarted == false)
                {
                    if (message.Type == MessageType.Text)
                    {
                        if (message.Text.ToLower() == "/start")
                        {
                            isStarted = true;
                            await botClient.SendTextMessageAsync(message.Chat, "Привет!");
                            return;
                        }
                    }
                }
                else
                {
                    if(isFace == false && isText == false)
                    {
                        if (message.Type == MessageType.Text)
                        {
                            if (message.Text.ToLower() == "/text")
                            {
                                isText = true;
                                await botClient.SendTextMessageAsync(message.Chat, "Отправь фото с текстом!");
                                return;
                            }
                        }
                        if (message.Type == MessageType.Text)
                        {
                            if (message.Text.ToLower() == "/face")
                            {
                                isFace = true;
                                await botClient.SendTextMessageAsync(message.Chat, "Отправь фото с лицом!");
                                return;
                            }
                        }
                    }
                    else if(message.Type == MessageType.Photo)
                    {
                        string msg = string.Empty;
                        var text = await botClient.GetFileAsync(message.Photo[message.Photo.Count() - 1].FileId);
                        Image img = Image.FetchFromUri($@"https://api.telegram.org/file/bot5326298021:AAF_P93aGZSAM3LHRZoQSFa0JzU0itqL7tM/" + text.FilePath);
                        if (isText)
                        {
                            msg = GetText(img);
                            if (msg != String.Empty)
                                await botClient.SendTextMessageAsync(message.Chat, msg);
                            else
                                await botClient.SendTextMessageAsync(message.Chat, "На фото нет текста!");
                            isText = false;
                            return;
                        }
                        if (isFace)
                        {
                            msg = GetFace(img);
                            if (msg != String.Empty)
                                await botClient.SendTextMessageAsync(message.Chat, msg);
                            else
                                await botClient.SendTextMessageAsync(message.Chat, "На фото нет лица!");
                            isFace = false;
                            return;
                        }
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(message.Chat, "Отправь фото!");
                        return;
                    }
                    
                }
                await botClient.SendTextMessageAsync(message.Chat, "Ошибка!");
            }
        }
        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            // Некоторые действия
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }

        public static string GetText(Image image)
        {
            info = string.Empty;
            ImageAnnotatorClient client = ImageAnnotatorClient.Create();
            IReadOnlyList<EntityAnnotation> textAnnotations = client.DetectText(image);

            if(textAnnotations.Count > 0)
                info += $"Description: {textAnnotations.First().Description}";
            return info;
        }
        public static string GetFace(Image image)
        {
            info = string.Empty;
            ImageAnnotatorClient client = ImageAnnotatorClient.Create();
            IReadOnlyList<FaceAnnotation> result = client.DetectFaces(image);
            foreach (FaceAnnotation face in result)
            {
                string poly = string.Join(" - ", face.BoundingPoly.Vertices.Select(v => $"({v.X}, {v.Y})"));
                info += $"Confidence: {(int)(face.DetectionConfidence * 100)}%; BoundingPoly: {poly}";
            }
            return info;
        }
        public static void Start()
        {
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", $@"h:\root\home\botserver-001\www\site1\info.json");
            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }, // receive all update types
            };
            bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );
        }
    }
}
