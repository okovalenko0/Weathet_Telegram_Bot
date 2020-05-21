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
        private static bool isWaitingServiceMessage = false;
        private static List<ReplyKeyboardMarkup> keyboards = new List<ReplyKeyboardMarkup>();
        private static TelegramBotClient Bot;

        private static void Main()
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

            Console.ReadLine();
            Bot.StopReceiving();
        }

        private static async void BotOnMessageReceived(object sender, MessageEventArgs e)
        {
            string curcase = "";
            Message msg = e.Message;
            if (msg.Type == MessageType.Text)
            {
                if (isWaitingCityName && (msg.Text != "Назад"))
                {
                    isWaitingCityName = false;
                    await CityNamesID(await MyWebRequest("http://sinoptik.com.ru/api/suggest.php?l=ru&q=" + msg.Text), msg);
                }
                else if (isWaitingServiceMessage && (msg.Text != "Назад"))
                {
                    isWaitingServiceMessage = false;
                    await Bot.SendTextMessageAsync(
                        -1001221331912,
                        "Тех. проблема #" + msg.MessageId +
                        "\nID: " + msg.From.Id +
                        "\nИмя: " + msg.From.FirstName +
                        "\nФамилия: " + msg.From.LastName +
                        "\nUsername: " + msg.From.Username +
                        "\nСодержание: " + msg.Text);
                    await Bot.SendTextMessageAsync(msg.From.Id, "Ваше сообщение было отправлено разработчику. \nСпасибо за отзыв!", replyMarkup: GetKeyboard(0));
                }
                else
                {
                    switch (msg.Text)
                    {
                        case "/start":
                            await Bot.SendTextMessageAsync(msg.Chat.Id, "Рад приветствовать тебя " + msg.From.FirstName + msg.From.LastName + " !", replyMarkup: GetKeyboard(0));
                            break;
                        case "/feedback":
                            goto case "Обратная связь";
                        case "Погода":
                            isWaitingCityName = true;
                            await Bot.SendTextMessageAsync(msg.Chat.Id, "Введите город, в котором желаете узнать погоду:", replyMarkup: GetKeyboard(3));
                            break;
                        case "Прогноз":
                            isWaitingCityName = true;
                            await Bot.SendTextMessageAsync(msg.Chat.Id, "Введите город, в котором желаете узнать погоду:", replyMarkup: GetKeyboard(3));
                            break;
                        case "Поддержка":
                            curcase = "Help";
                            await Bot.SendTextMessageAsync(msg.Chat.Id, "Поддержка:", replyMarkup: GetKeyboard(2));
                            break;
                        case "Настройки":
                            curcase = "Settings";
                            await Bot.SendTextMessageAsync(msg.Chat.Id, "Настройки бота:", replyMarkup: GetKeyboard(1));
                            break;
                        case "Обратная связь":
                            isWaitingServiceMessage = true;
                            curcase = "Msgtome";
                            await Bot.SendTextMessageAsync(msg.Chat.Id, "Отправьте сообщение с проблемой мне, я передам его создателю.");
                            break;
                        case "Справка":
                            await Info(msg);
                            break;
                        case "Назад":
                            isWaitingCityName = false;
                            isWaitingServiceMessage = false;
                            await Bot.SendTextMessageAsync(msg.Chat.Id, "Возвращаемся назад", replyMarkup: Back(curcase));
                            break;
                    }
                }
            }
            else if (msg.Type == MessageType.Location)
            {
                await CityLocationID(await MyWebRequest("https://sinoptik.com.ru/api/location.php?s=false&lat=" + msg.Location.Latitude + "&lon=" + msg.Location.Longitude), msg);
            }
            else
                await Bot.SendTextMessageAsync(msg.Chat.Id, "С этим я ещё работать не умею.", replyMarkup: GetKeyboard(0));
        }

        private static async Task<string> MyWebRequest(string msg)
        {
            return await Task.Run(() => Metod(msg));
        }

        private static string Metod(string msg)
        {
            string responseFromServer = "UNKNOWN ERROR";
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

        private static async Task CityNamesID(string apiRequest, Message msg)
        {
            if (apiRequest != "[]")
            {
                ReplyKeyboardMarkup test = GetKeyboard(4);
                string[] mass = JSON_API_Decryptor.GetAllItems(apiRequest);
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
                await Bot.SendTextMessageAsync(msg.From.Id, "Выберите ваш город:", replyMarkup: GetKeyboard(4));
            }
            else
                await Bot.SendTextMessageAsync(msg.From.Id, "Указанный город не найден!", replyMarkup: GetKeyboard(0));
        }
        private static async Task CityLocationID(string apiRequest, Message msg)
        {
            string ID = JSON_API_Decryptor.ExtractSubField(apiRequest, "id");
            string weatherRequest = await MyWebRequest("https://sinoptik.com.ru/api/weather.php?l=ru&id=" + ID);
            if (apiRequest != "[]")
            {
                DateTime Now = DateTime.Now;
                Now = Now.Date;
                string titleIn = JSON_API_Decryptor.GetStringAtPath(weatherRequest, "r/titleIn");
                string min = JSON_API_Decryptor.GetStringAtPath(weatherRequest, "tabs/[0]/min");
                string max = JSON_API_Decryptor.GetStringAtPath(weatherRequest, "tabs/[0]/max");
                string textf = JSON_API_Decryptor.GetStringAtPath(weatherRequest, "days/[0]/tf");
                string sr = JSON_API_Decryptor.GetStringAtPath(weatherRequest, "days/[0]/sr");
                string ss = JSON_API_Decryptor.GetStringAtPath(weatherRequest, "days/[0]/ss");
                string tn = JSON_API_Decryptor.GetStringAtPath(weatherRequest, "tn");
                string p = JSON_API_Decryptor.GetStringAtPath(weatherRequest, "days/[0]/hours/[0]/p");
                string w = JSON_API_Decryptor.GetStringAtPath(weatherRequest, "days/[0]/hours/[0]/w");
                string wd = JSON_API_Decryptor.GetStringAtPath(weatherRequest, "days/[0]/hours/[0]/wd");
                string ws = JSON_API_Decryptor.GetStringAtPath(weatherRequest, "days/[0]/hours/[0]/ws");
                string pp = JSON_API_Decryptor.GetStringAtPath(weatherRequest, "days/[0]/hours/[0]/pp");
                await Bot.SendTextMessageAsync(msg.From.Id,
                    "Погода в " + titleIn + " на " + Now +
                    "\n\nТемпература на сегодня: от " + min + "℃ до " + max + "℃" +
                    "\nТемпература сейчас: " + tn +
                    "\nДавление: " + p +
                    "\nВлажность: " + w + "%" +
                    "\nВетер: " + wd + " " + ws +
                    "\nВероятность осадков: " + pp + "%" +
                    "\n\nПрогноз от синоптиков: \n" + textf
                    );
            }
            else
                await Bot.SendTextMessageAsync(msg.From.Id, "Сервер sinoptik.com.ru в данный момент не доступен.", replyMarkup: GetKeyboard(0));
        }

        //private static async Task WeatherToday(string apiRequest, Message msg)
        //{
        //    //


        //    ///////////////////////////////////////////////////////////////////////////////////////////
        //    //Console.WriteLine(JSON_API_Decryptor.GetStringAtPath(responsemy, "days/[0]/hours/[0]/h"));
        //    //string sourceString = "";
        //    //using (FileStream fs = new FileStream("TEST_JSON_CITIES.file", FileMode.Open))
        //    //{
        //    //    using (StreamReader sr = new StreamReader(fs))
        //    //    {
        //    //        sourceString = sr.ReadToEnd();
        //    //    }
        //    //}
        //    //foreach (string item in JSON_API_Decryptor.GetAllItems(sourceString))
        //    //{
        //    //    Console.WriteLine("----------------------------");
        //    //    Console.WriteLine(item);
        //    //}
        //}

        private static ReplyKeyboardMarkup GetKeyboard(int key)
        {
            ReplyKeyboardMarkup mainmenu = new ReplyKeyboardMarkup(){Keyboard = new[]
            {
                new[] { new KeyboardButton("Погода"), new KeyboardButton("Прогноз")},
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
            ReplyKeyboardMarkup locationsend = new ReplyKeyboardMarkup(new[]
            {
                 KeyboardButton.WithRequestLocation("Отправить своё местоположение"),
                 new KeyboardButton("Назад")
            });

            ReplyKeyboardMarkup availableCities = new ReplyKeyboardMarkup();

            keyboards.Add(mainmenu);
            keyboards.Add(settings);
            keyboards.Add(help);
            keyboards.Add(locationsend);
            keyboards.Add(availableCities);
            foreach (ReplyKeyboardMarkup s in keyboards)
            {
                s.ResizeKeyboard = true;
            }
            return keyboards.ElementAt(key);
        }

        private static ReplyKeyboardMarkup Back(string curcase)
        {
            if ((curcase == "Help") || (curcase == "Settings") || (curcase == ""))
                return GetKeyboard(0);
            else if ((curcase == "Notifications") || (curcase == "Locs") || (curcase == "Language"))
                return GetKeyboard(1);
            else if (curcase == "Msgtome")
                return GetKeyboard(2);
            else
                return GetKeyboard(0);
        }

        private static async Task Info(Message msg)
        {
            string infoString = "";
            using (FileStream fs = new FileStream("INFO.file", FileMode.Open))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    infoString = sr.ReadToEnd();
                }
            }
            await Bot.SendTextMessageAsync(msg.From.Id, infoString, replyMarkup: GetKeyboard(2));
        }

        private static async void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            await Bot.SendTextMessageAsync(
                -1001221331912,
                "Получена программная: " +
                "\n" + receiveErrorEventArgs.ApiRequestException.ErrorCode + 
                "\n" + receiveErrorEventArgs.ApiRequestException.Message);
        }
    }
}
