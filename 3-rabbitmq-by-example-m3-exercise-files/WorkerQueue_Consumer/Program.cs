using System;
using RabbitMQ.Client;

namespace RabbitMQ.Examples
{
    // to better test the multiple customers case, run two instances of this project after having something in the queue and see how load will be split over them
    // either start two vs instances lol , or go to bin/debug and run the exe twice
    public class Program
    {
        private static ConnectionFactory _factory;
        private static IConnection _connection;
        
        private const string QueueName = "WorkerQueue_Queue";
        
        static void Main()
        {
            Receive();

            Console.ReadLine();
        }

        public static void Receive()
        {
            _factory = new ConnectionFactory { HostName = "localhost", UserName = "guest", Password = "guest" };
            using (_connection = _factory.CreateConnection())
            {
                using (var channel = _connection.CreateModel()) // creating model
                {
                    channel.QueueDeclare(QueueName, true, false, false, null);
                    // quality of service
                    // second param is fetch count 
                    // rabbitmq wont dispatch a new message to consumer until that consumer is finsihed consuming that message the consumer is on.
                    // this helps better with load balancing where you might get one consumer that is heavily bounded and the other is lightly utilized
                    channel.BasicQos(0, 1, false);

                    // craeting a consumer and hooking up the channel
                    var consumer = new QueueingBasicConsumer(channel);
                    // second param is if we want to notify for ack
                    channel.BasicConsume(QueueName, false, consumer); 

                    while (true)
                    {
                        var ea = consumer.Queue.Dequeue();
                        var message = (Payment)ea.Body.DeSerialize(typeof(Payment));
                        // sending acknowledgement back and telling it you can disgard it from queue, will not get another message until our ack is received by the other end
                        channel.BasicAck(ea.DeliveryTag, false);

                        Console.WriteLine("----- Payment Processed {0} : {1}", message.CardNumber, message.AmountToPay);
                    }
                }
            }
        }
    }
}
