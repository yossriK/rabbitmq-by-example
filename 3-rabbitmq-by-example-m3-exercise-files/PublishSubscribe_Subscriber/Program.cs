using System;
using RabbitMQ.Client;

namespace RabbitMQ.Examples
{
    class Program
    {
        private static ConnectionFactory _factory;
        private static IConnection _connection;
        private static QueueingBasicConsumer _consumer;

        private const string ExchangeName = "PublishSubscribe_Exchange";

        static void Main()
        {
            _factory = new ConnectionFactory { HostName = "localhost", UserName = "guest", Password = "guest" };
            using (_connection = _factory.CreateConnection())
            {
                using (var channel = _connection.CreateModel())
                {
               
                    var queueName = DeclareAndBindQueueToExchange(channel);
                    // second param is queue indicating we not waiting for ack in order to get another message
                    channel.BasicConsume(queueName, true, _consumer);

                    while (true)
                    {
                        var ea = _consumer.Queue.Dequeue();
                        var message = (Payment)ea.Body.DeSerialize(typeof(Payment));

                        Console.WriteLine("----- Payment Processed {0} : {1}", message.CardNumber, message.AmountToPay);
                    }
                }
            }
        }


        private static string DeclareAndBindQueueToExchange(IModel channel)
        {
            // declaring the exchange, if it is already there then nothign will happen
            channel.ExchangeDeclare(ExchangeName, "fanout");
            // getting the queue name that is system generated, queue is then created and bound to that exchange
            // so here queue I guess is created froom the custoemr side bound to that specific exchange
            var queueName = channel.QueueDeclare().QueueName;
            channel.QueueBind(queueName, ExchangeName, "");
            _consumer = new QueueingBasicConsumer(channel);
            return queueName;
        }
    }
}
