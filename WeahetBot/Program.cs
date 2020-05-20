using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using System.Collections.Generic;
using WeahetBot;

namespace WeatherBot
{
    public static class Program
    {
        private static bool isWaitingCityName = false;
        private static TelegramBotClient Bot;

        private static List<ReplyKeyboardMarkup> keyboards = new List<ReplyKeyboardMarkup>();

        public static int LocationID(float lat, float lon)
        {
            int ID = 0;
            // Create a request for the URL.
            WebRequest request = WebRequest.Create(
              "https://sinoptik.de/api/location.php?s=false&lat=" + lat + "&lon=" + lon);
            // If required by the server, set the credentials.
            request.Credentials = CredentialCache.DefaultCredentials;
            // Get the response.
            WebResponse response = request.GetResponse();
            // Get the stream containing content returned by the server.
            // The using block ensures the stream is automatically closed.
            using (Stream dataStream = response.GetResponseStream())
            {
                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                string responseFromServer = reader.ReadToEnd();
                responseFromServer.Trim();
                string[] display = responseFromServer.Split(new char[] { ',' });
                // Display the content.
                foreach (string s in display)
                {
                    if (s.IndexOf("id") != -1)
                    {
                        ID =  Convert.ToInt32(s.Remove(0, 5));
                    }
                }
            }
            // Close the response.
            response.Close();
            return ID;
        }

        private static KeyboardButton[][] GetReplyKeyboard(string[] stringArray)
        {
            KeyboardButton[][] keyboardReply = new KeyboardButton[1][];
            KeyboardButton[] keyboardButtons = new KeyboardButton[stringArray.Length+1];
            for (var i = 0; i < stringArray.Length; i++)
            {
                keyboardButtons[i] = new KeyboardButton
                {
                    Text = stringArray[i]
                };
                if (i == stringArray.Length - 1)
                {
                    keyboardButtons[i+1] = new KeyboardButton
                    {
                        Text = "Назад"
                    };
                };
            }
            keyboardReply[0] = keyboardButtons;
            return keyboardReply;
        }

        static async Task<string> MyWebRequest(string msg)
        {
            return await Task.Run(() => Metod(msg));
        }

        public static string Metod(string msg)
        {
            string responseFromServer = "ERROR NIKITA AHTUNG";
            WebRequest request = WebRequest.Create(msg);
            request.Credentials = CredentialCache.DefaultCredentials;
            WebResponse response = request.GetResponse();
            using (Stream dataStream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(dataStream);
                responseFromServer = reader.ReadToEnd();
            }
            response.Close();
            return responseFromServer;
        }

        public static void Main()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            Bot = new TelegramBotClient("1251334082:AAHqEJrTsFFczl8snVaanvOMXHa7k3TJW28");
            User me = Bot.GetMeAsync().Result;
            Console.Title = me.Username;
            
            Bot.OnMessage += BotOnMessageReceived;
            Bot.OnMessageEdited += BotOnMessageReceived;
            //Bot.OnCallbackQuery += BotOnCallbackQueryReceived;
            //Bot.OnInlineQuery += BotOnInlineQueryReceived;
            //Bot.OnInlineResultChosen += BotOnChosenInlineResultReceived;
            Bot.OnReceiveError += BotOnReceiveError;

            Console.OutputEncoding = System.Text.Encoding.Unicode;

            Bot.StartReceiving(Array.Empty<UpdateType>());
            Console.WriteLine($"Start listening for @{me.Username}");

            TestPrint();

            Console.ReadLine();
            Bot.StopReceiving();
        }

        private static void TestPrint()
        {
            int ID = 303023954;
            string responsemy;
            // Create a request for the URL.
            WebRequest request = WebRequest.Create("https://sinoptik.com.ru/api/weather.php?l=ru&id=" + ID);
            // If required by the server, set the credentials.
            request.Credentials = CredentialCache.DefaultCredentials;
            // Get the response.
            WebResponse response = request.GetResponse();
            // Get the stream containing content returned by the server.
            // The using block ensures the stream is automatically closed.
            using (Stream dataStream = response.GetResponseStream())
            {
                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                string responseFromServer = reader.ReadToEnd();
                responsemy = responseFromServer;
                //Console.WriteLine(responseFromServer);
            }
            // Close the response.
            response.Close();

            ///////////////////////////////////////////////////////////////////////////////////////////
            //Console.WriteLine(JSON_API_Decryptor.GetStringAtPath(responsemy, "days/[0]/hours/[0]/h"));
            //string sourceString = "";
            //using (FileStream fs = new FileStream("TEST_JSON_CITIES.file", FileMode.Open))
            //{
            //    using (StreamReader sr = new StreamReader(fs))
            //    {
            //        sourceString = sr.ReadToEnd();
            //    }
            //}
            //foreach (string item in JSON_API_Decryptor.GetAllItems(sourceString))
            //{
            //    Console.WriteLine("----------------------------");
            //    Console.WriteLine(item);
            //}
        }

        public static ReplyKeyboardMarkup GetKeyboard(int key)
        {
            ReplyKeyboardMarkup mainmenu = new ReplyKeyboardMarkup(){Keyboard = new[]
            {
                new[] { new KeyboardButton("Погода сейчас"), new KeyboardButton("Прогноз погоды")},
                new[] { new KeyboardButton("Поддержка"), new KeyboardButton("Настройки") }
            }};
            ReplyKeyboardMarkup settings = new ReplyKeyboardMarkup()
            {
                Keyboard = new[]
                {
                    new[] { new KeyboardButton("Локации"), new KeyboardButton("Уведомления"), new KeyboardButton("Язык") },
                    new[] { new KeyboardButton("Назад") }
                }
            };
            ReplyKeyboardMarkup help = new ReplyKeyboardMarkup()
            {
                Keyboard = new[]
                {
                    new[] { new KeyboardButton("Справка"), new KeyboardButton("Обратная связь") },
                    new[] { new KeyboardButton("Назад") }
                }
            };
            ReplyKeyboardMarkup location = new ReplyKeyboardMarkup()
            {
                Keyboard = new[]
                {
                    new[] { new KeyboardButton("Добавить"), new KeyboardButton("Удалить") },
                    new[] { new KeyboardButton("Назад") }
                }
            };
            ReplyKeyboardMarkup locationsend = new ReplyKeyboardMarkup(new[]
            {
                 KeyboardButton.WithRequestLocation("Отправить своё местоположение"),
                 new KeyboardButton("Назад")
            });

            ReplyKeyboardMarkup availableCities = new ReplyKeyboardMarkup();

            keyboards.Add(mainmenu);
            keyboards.Add(settings);
            keyboards.Add(help);
            keyboards.Add(location);
            keyboards.Add(locationsend);
            keyboards.Add(availableCities);
            foreach (ReplyKeyboardMarkup s in keyboards)
            {
                s.ResizeKeyboard = true;
            }
            return keyboards.ElementAt(key);
        }

        private static KeyboardButton[][] kekke(string[] stringArray)
        {
            KeyboardButton[][] keyboardReply = new KeyboardButton[1][];
            KeyboardButton[] keyboardButtons = new KeyboardButton[stringArray.Length + 1];
            for (var i = 0; i < stringArray.Length; i++)
            {
                keyboardButtons[i] = new KeyboardButton
                {
                    Text = stringArray[i]
                };
                if (i == stringArray.Length - 1)
                {
                    keyboardButtons[i + 1] = new KeyboardButton
                    {
                        Text = "Назад"
                    };
                };
            }
            keyboardReply[0] = keyboardButtons;
            return keyboardReply;
        }



        public static ReplyKeyboardMarkup Back(string curcase)
        {
            if ((curcase == "Help") || (curcase == "Settings") || (curcase == ""))
                return GetKeyboard(0);
            else if ((curcase == "Notifications") || (curcase == "Locs") || (curcase == "Language"))
                return GetKeyboard(1);
            else if (curcase == "Msgtome")
                return GetKeyboard(2);
            else if ((curcase == "Addloc") || (curcase == "Delloc"))
                return GetKeyboard(3);
            else
                return GetKeyboard(0);
        }
        


        private static async void BotOnMessageReceived(object sender, MessageEventArgs e)
        {
            string curcase = "";
            Message msg = e.Message;
            // HACK:
            if (msg.From.Id == 699429390 || true)
            {
                if (msg.Type == MessageType.Text)
                {
                    if (isWaitingCityName)
                    {
                        string kaka = await MyWebRequest("http://sinoptik.com.ru/api/suggest.php?l=ru&q=" + msg.Text);
                        if (kaka != "[]")
                        {
                            ReplyKeyboardMarkup test = GetKeyboard(5);
                            string[] mass = JSON_API_Decryptor.GetAllItems(kaka);
                            KeyboardButton[][] buttons = new KeyboardButton[mass.Length + 1][];

                            for (int i = 0; i < buttons.GetLength(0); i++)
                            {
                                buttons[i] = new KeyboardButton[] { new KeyboardButton() };
                            }
                            for (int i = 0; i < mass.GetLength(0); i++)
                            {
                                string str = JSON_API_Decryptor.ExtractSubField(mass[i], "title") + ", " + JSON_API_Decryptor.ExtractSubField(mass[i], "descr");
                                buttons[i][0].Text = str;
                            }
                            buttons[mass.Length][0].Text = "Назад";
                            test.Keyboard = buttons;
                            await Bot.SendTextMessageAsync(msg.From.Id, "Выберите ваш город:", replyMarkup: GetKeyboard(5));

                        }
                        else
                            await Bot.SendTextMessageAsync(msg.From.Id, "Указанный город не найден!", replyMarkup: GetKeyboard(0));
                        isWaitingCityName = false;
                    }
                    else
                    {
                        switch (msg.Text)
                        {
                            case "/start":
                                await Bot.SendTextMessageAsync(msg.Chat.Id, "Рад приветствовать тебя " + msg.From.FirstName + msg.From.LastName + " !", replyMarkup: GetKeyboard(0));
                                break;
                            case "Погода сейчас":
                                isWaitingCityName = true;
                                await Bot.SendTextMessageAsync(msg.Chat.Id, "Введите город, в котором желаете узнать погоду:", replyMarkup: GetKeyboard(4));
                                break;
                            case "Прогноз погоды":
                                isWaitingCityName = true;
                                await Bot.SendTextMessageAsync(msg.Chat.Id, "Введите город, в котором желаете узнать погоду:", replyMarkup: GetKeyboard(4));
                                break;
                            case "Радар":
                                await Bot.SendTextMessageAsync(msg.Chat.Id, "Тут мб будет радар", replyMarkup: GetKeyboard(0));
                                break;
                            case "Поддержка":
                                curcase = "Help";
                                await Bot.SendTextMessageAsync(msg.Chat.Id, "Здесь вы сможете связаться с разработчиком:", replyMarkup: GetKeyboard(2));
                                break;
                            case "Настройки":
                                curcase = "Settings";
                                await Bot.SendTextMessageAsync(msg.Chat.Id, "Настройки бота:", replyMarkup: GetKeyboard(1));
                                break;
                            case "Назад":
                                await Bot.SendTextMessageAsync(msg.Chat.Id, "Возвращаемся назад", replyMarkup: Back(curcase));
                                break;
                            case "Уведомления":
                                curcase = "Notifications";
                                await Bot.SendTextMessageAsync(msg.Chat.Id, "Главное меню:", replyMarkup: GetKeyboard(0));
                                break;
                            case "Язык":
                                curcase = "Language";
                                await Bot.SendTextMessageAsync(msg.Chat.Id, "Главное меню:", replyMarkup: GetKeyboard(0));
                                break;
                            case "Локации":
                                curcase = "Locs";
                                await Bot.SendTextMessageAsync(msg.Chat.Id, "Главное меню:", replyMarkup: GetKeyboard(0));
                                break;
                            case "Удалить":
                                curcase = "Delloc";
                                await Bot.SendTextMessageAsync(msg.Chat.Id, "Главное меню:", replyMarkup: GetKeyboard(0));
                                break;
                            case "Добавить":
                                curcase = "Addloc";
                                await Bot.SendTextMessageAsync(msg.Chat.Id, "Test");
                                break;
                            case "Обратная связь":
                                curcase = "Msgtome";
                                await Bot.SendTextMessageAsync(msg.Chat.Id, "Отправьте сообщение с проблемой мне, я передам его создателю.");
                                break;
                            case "Nikita":
                                await Bot.SendTextMessageAsync(msg.Chat.Id, "1", replyMarkup: GetKeyboard(5));
                                break;
                        }
                    }
                }
                else if (msg.Type == MessageType.Location)
                {
                    int ID = LocationID(msg.Location.Latitude, msg.Location.Longitude);
                    await Bot.SendTextMessageAsync(msg.Chat.Id, Convert.ToString(ID), replyMarkup: GetKeyboard(4));
                }
                else
                    await Bot.SendTextMessageAsync(msg.Chat.Id, "С этим я ещё работать не умею.", replyMarkup: GetKeyboard(0));
            }
            else
                await Bot.SendTextMessageAsync(msg.Chat.Id, "К сожалению, только администратор имеет право писать мне!");
        }
        private static void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            Console.WriteLine
            (
                "Received error: {0} — {1}",
                receiveErrorEventArgs.ApiRequestException.ErrorCode,
                receiveErrorEventArgs.ApiRequestException.Message
            );
        }
    }
}
