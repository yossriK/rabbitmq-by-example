using System;
using RabbitMQ.Client;

namespace RabbitMQ.Examples
{
    class Program
    {
        // this is just standard example here to see the functionality
        // we got the sender and reciever in same file lol, in 
        // real life scenario they would be in 2 different projects; i mean publisher would publish
        // to a different project/solution
        private static ConnectionFactory _factory;
        private static IConnection _connection;
        private static IModel _model;
                
        private const string QueueName = "StandardQueue_ExampleQueue";

        public static void Main()
        {            
            var payment1 = new Payment { AmountToPay = 25.0m, CardNumber = "1234123412341234", Name = "Mr S Haunts" };
            var payment2 = new Payment { AmountToPay = 5.0m, CardNumber = "1234123412341234", Name = "Mr S Haunts" };
            var payment3 = new Payment { AmountToPay = 2.0m, CardNumber = "1234123412341234", Name = "Mr S Haunts" };
            var payment4 = new Payment { AmountToPay = 17.0m, CardNumber = "1234123412341234", Name = "Mr S Haunts" };
            var payment5 = new Payment { AmountToPay = 300.0m, CardNumber = "1234123412341234", Name = "Mr S Haunts" };
            var payment6 = new Payment { AmountToPay = 350.0m, CardNumber = "1234123412341234", Name = "Mr S Haunts" };
            var payment7 = new Payment { AmountToPay = 295.0m, CardNumber = "1234123412341234", Name = "Mr S Haunts" };
            var payment8 = new Payment { AmountToPay = 5625.0m, CardNumber = "1234123412341234", Name = "Mr S Haunts" };
            var payment9 = new Payment { AmountToPay = 5.0m, CardNumber = "1234123412341234", Name = "Mr S Haunts" };
            var payment10 = new Payment { AmountToPay = 12.0m, CardNumber = "1234123412341234", Name = "Mr S Haunts" };
                        
            CreateQueue();            
                        
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
                                 
            Recieve();

            Console.ReadLine();
        }

        private static void CreateQueue()
        {
            _factory = new ConnectionFactory { HostName = "localhost", UserName = "guest", Password = "guest"};
            _connection = _factory.CreateConnection();
            _model = _connection.CreateModel(); 
                     
            _model.QueueDeclare(QueueName, true, false, false, null);            
        }

        private static void SendMessage(Payment message)
        {   // serializing and sending message
            // we using default exchange here, look at first param how it was left empty
            _model.BasicPublish("", QueueName, null, message.Serialize());
            Console.WriteLine(" [x] Payment Message Sent : {0} : {1} : {2}", message.CardNumber, message.AmountToPay, message.Name);            
        }

        public static void Recieve()
        {   // setting queuing basic consumer
            var consumer = new QueueingBasicConsumer(_model);            
            var msgCount = GetMessageCount(_model, QueueName);
            
            _model.BasicConsume(QueueName, true, consumer); // telling rabbit that I want to consume
                    
            var count = 0;
            
            while (count < msgCount)
            {                               
                var message = (Payment)consumer.Queue.Dequeue().Body.DeSerialize(typeof(Payment));

                Console.WriteLine("----- Received {0} : {1} : {2}", message.CardNumber, message.AmountToPay, message.Name);
                count++;
            }                   
        }

        private static uint GetMessageCount(IModel channel, string queueName)
        {
            var results = channel.QueueDeclare(queueName, true, false, false, null);
            return results.MessageCount;                
        }
    }
}
