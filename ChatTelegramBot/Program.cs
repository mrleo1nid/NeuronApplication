using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NeuralBotBase.Helpers;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace ChatTelegramBot
{
    class Program
    {
        private static TelegramBotClient client;
        private static NeuronHelper neuronHelper;
        private const string token = "";
        static void Main(string[] args)
        {
            neuronHelper = new NeuronHelper();
            // Console.WriteLine($"Обучаем бота");
            // neuronHelper.LearnBot();
            // Console.WriteLine($"Бот успешно обучен");
            WebProxy wp = new WebProxy("50.113.187.160:443", true);
            client = new TelegramBotClient(token,wp);
            client.OnMessage += BotOnMessageReceived;
            client.OnMessageEdited += BotOnMessageReceived;
            client.StartReceiving();
            var me = client.GetMeAsync().Result;
            Console.WriteLine($"Бот {me.Username} начинает своё вещание :D");
            Console.ReadLine();
        }
        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;
            Console.WriteLine($"Пришло от юзера [{message.Chat.Username}] : {message.Text}");
            if (message?.Type == MessageType.Text)
            {
                var messagesize = message.Text.Split(' ').Length;
                if (message.Text.StartsWith("/learn"))
                {
                    var text = message.Text.Substring(6);
                    var textsize = text.Split(':');
                    if (textsize.Length<2)
                    {
                        await client.SendTextMessageAsync(message.Chat.Id, "Обучающее сообщение должно иметь формат- строка сообщения:строка ответа");
                        return;
                    }
                    else
                    {
                        var requestlen = textsize[0].Split(' ').Length;
                        var responcelen = textsize[1].Split(' ').Length;
                        if (responcelen>10)
                        {
                            await client.SendTextMessageAsync(message.Chat.Id, "Ответ для обучения должен быть меньше 10 слов");
                            return;
                        }
                        if (requestlen > 10)
                        {
                            await client.SendTextMessageAsync(message.Chat.Id, "Вопрос для обучения должен быть меньше 10 слов");
                            return;
                        }
                        neuronHelper.LearnFromString(textsize[0],textsize[1]);
                        await client.SendTextMessageAsync(message.Chat.Id, "Бот успешно обучен");
                        Console.WriteLine($"Бот обучен юзером [{message.Chat.Username}] : {textsize[0]}:{textsize[1]}");
                        return;
                    }
                }
                else if (message.Text.StartsWith("/help"))
                {
                    await client.SendTextMessageAsync(message.Chat.Id, "Пока не готово");
                    return;
                }
                else if (messagesize<=10)
                {
                    var mesg = neuronHelper.CreateMessage(message.Text);
                    if (string.IsNullOrEmpty(mesg))
                    {
                        mesg = "Бот не смог создать ответ";
                    }
                    Console.WriteLine($"Ответ бота юзеру [{message.Chat.Username}] : {mesg}");
                    await client.SendTextMessageAsync(message.Chat.Id, mesg);
                    return;
                }
                else
                {
                    await client.SendTextMessageAsync(message.Chat.Id, "Сообщение пока должно быть меньше 10 слов");
                    return;
                }
            }
        }
    }
}
