using System;
using System.Text;
using Payments.Models;
using RabbitMQ.Client;

namespace Payments.RabbitMQ
{
    public class RabbitMQDirectClient
    {
        private IConnection _connection;
        private IModel _channel;
        private string _replyQueueName;
        private QueueingBasicConsumer _consumer;

        public void CreateConnection()
        {
            var factory = new ConnectionFactory { HostName = "localhost", UserName = "guest", Password = "guest" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            // rpc_reply is routine key here
            _replyQueueName = _channel.QueueDeclare("rpc_reply", true, false, false, null);           
            _consumer = new QueueingBasicConsumer(_channel);
            _channel.BasicConsume(_replyQueueName, true, _consumer);
        }

        public void Close()
        {
            _connection.Close();
        }

        public string MakePayment(CardPayment payment)
        {
            // correleation id is useful when talking between micro services, we hence make sure we are recieving correct messages/replies
            var corrId = Guid.NewGuid().ToString();
            var props = _channel.CreateBasicProperties();
            props.ReplyTo = _replyQueueName;
            props.CorrelationId = corrId;

            // defaulting to default exchange
            _channel.BasicPublish("", "rpc_queue", props, payment.Serialize());

            // we made a payment then started wauiting for a response
            while (true)
            {
                var ea = _consumer.Queue.Dequeue();
                // checking that is the correct correlation id of the stuff that I just sent
                if (ea.BasicProperties.CorrelationId != corrId) continue;

                var authCode = Encoding.UTF8.GetString(ea.Body);                    
                return authCode;
            }
        }
    }
}
