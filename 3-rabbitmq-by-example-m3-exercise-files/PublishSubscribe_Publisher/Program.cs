﻿

using System;
using RabbitMQ.Client;

namespace RabbitMQ.Examples
{
    class Program
    {
        private static ConnectionFactory _factory;
        private static IConnection _connection;
        private static IModel _model;

        private const string ExchangeName = "PublishSubscribe_Exchange";

        static void Main()
        {
            // set up and send some payments
            var payment1 = new Payment { AmountToPay = 25.0m, CardNumber = "1234123412341234" };
            var payment2 = new Payment { AmountToPay = 5.0m, CardNumber = "1234123412341234" };
            var payment3 = new Payment { AmountToPay = 2.0m, CardNumber = "1234123412341234" };
            var payment4 = new Payment { AmountToPay = 17.0m, CardNumber = "1234123412341234" };
            var payment5 = new Payment { AmountToPay = 300.0m, CardNumber = "1234123412341234" };
            var payment6 = new Payment { AmountToPay = 350.0m, CardNumber = "1234123412341234" };
            var payment7 = new Payment { AmountToPay = 295.0m, CardNumber = "1234123412341234" };
            var payment8 = new Payment { AmountToPay = 5625.0m, CardNumber = "1234123412341234" };
            var payment9 = new Payment { AmountToPay = 5.0m, CardNumber = "1234123412341234" };
            var payment10 = new Payment { AmountToPay = 12.0m, CardNumber = "1234123412341234" };

            CreateConnection();

            SendMessage(payment1);
            SendMessage(payment2);
            SendMessage(payment3);
            SendMessage(payment4);
            SendMessage(payment5);
            SendMessage(payment6);
            SendMessage(payment7);
            SendMessage(payment8);
            SendMessage(payment9);
            SendMessage(payment10);

            Console.ReadLine();
        }

        private static void CreateConnection()
        {
            _factory = new ConnectionFactory { HostName = "localhost", UserName = "guest", Password = "guest" };
            _connection = _factory.CreateConnection();
            _model = _connection.CreateModel();
            // we can make this durable so it is persistent to the disk
            _model.ExchangeDeclare(ExchangeName, "fanout", false);
        }

        private static void SendMessage(Payment message)
        {   // we ingroned the routing key which was the queue name. instead we sent the exchange name
            // now only queues bound to this exchange will get the messages
            // each subscriber will get all the messages now
            _model.BasicPublish(ExchangeName, "", null, message.Serialize());
            Console.WriteLine(" Payment Sent {0}, £{1}", message.CardNumber, message.AmountToPay);             
        }
    }
}
