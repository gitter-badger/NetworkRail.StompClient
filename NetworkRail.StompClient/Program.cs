using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using Apache.NMS;

namespace NetworkRail.StompClient
{
    class Program
    {
        static void Main(string[] args)
        {
            IConnectionFactory factory = new NMSConnectionFactory(new Uri("stomp:tcp://datafeeds.networkrail.co.uk:61618"));

            using (IConnection connection = factory.CreateConnection(ConfigurationManager.AppSettings["FeedUsername"], ConfigurationManager.AppSettings["FeedPassword"]))
            {
                connection.ClientId = ConfigurationManager.AppSettings["FeedUsername"];
                connection.Start();

                using (ISession session = connection.CreateSession())
                {
                    IDestination movementDestination = session.GetDestination("topic://" + "TRAIN_MVT_EF_TOC");
                    IMessageConsumer movementConsumer = session.CreateConsumer(movementDestination);
                    movementConsumer.Listener += new MessageListener(OnMovementMessage);

                    IDestination scheduleDestination = session.GetDestination("topic://" + "VSTP_ALL");
                    IMessageConsumer scheduleConsumer = session.CreateConsumer(scheduleDestination);
                    scheduleConsumer.Listener += new MessageListener(OnScheduleMessage);
                    
                    Console.WriteLine("Consumer started, waiting for messages... (Press ENTER to stop.)");

                    Console.ReadLine();
                    connection.Close();
                }
            }
        }

        private static void OnMovementMessage(IMessage message)
        {
            try
            {
                Console.WriteLine("Movement message received.");

                ITextMessage msg = (ITextMessage)message;
                message.Acknowledge();

                PostJson(msg.Text, MessageType.Movement);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("---");
                Console.WriteLine(ex.InnerException);
                Console.WriteLine("---");
                Console.WriteLine(ex.InnerException.Message);
            }
        }

        private static void OnScheduleMessage(IMessage message)
        {
            try
            {
                Console.WriteLine("Schedule message received.");

                ITextMessage msg = (ITextMessage)message;
                message.Acknowledge();

                PostJson(msg.Text, MessageType.Schedule);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("---");
                Console.WriteLine(ex.InnerException);
                Console.WriteLine("---");
                Console.WriteLine(ex.InnerException.Message);
            }
        }

        private static void PostJson(string json, MessageType messageType)
        {
            string postUrl;

            switch (messageType)
            {
                case MessageType.Movement:
                    postUrl = ConfigurationManager.AppSettings["MovementPostUrl"];
                    break;
                default:
                    postUrl = ConfigurationManager.AppSettings["SchedulePostUrl"];
                    break;
            }

            Byte[] bytes = Encoding.UTF8.GetBytes(json);

            var httpRequest = (HttpWebRequest) WebRequest.Create(postUrl);

            httpRequest.ContentType = "plain/json";
            httpRequest.Method = "POST";

            using (var streamWriter = httpRequest.GetRequestStream())
            {
                streamWriter.Write(bytes, 0, bytes.Length);
            }

            var httpReponse = (HttpWebResponse) httpRequest.GetResponse();

            using (var streamReader = new StreamReader(httpReponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                Console.WriteLine(result);
            }
        }
    }
}
