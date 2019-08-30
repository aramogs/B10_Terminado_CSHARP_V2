using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RabbitMQ.Client;
using Newtonsoft.Json;

namespace EjemploSAP1.rabbitMq
{
    class ProducerToEx
    {


        protected IModel channel;
        protected IConnection Connection;
        protected string QueueName;
        protected ConnectionFactory connectionFactory;

        public ProducerToEx()
        {

            
            connectionFactory = new ConnectionFactory();
            connectionFactory.UserName = Program.myConf.userMsg;
            connectionFactory.Password = Program.myConf.passMsg;
            
            connectionFactory.Port = 5672;
            connectionFactory.VirtualHost = "/";

            connectionFactory.HostName = Program.myConf.serverMsg;

            Connection = connectionFactory.CreateConnection();

            channel = Connection.CreateModel();
            bool durable = true;
            channel.QueueDeclare(QueueName, durable, false, false, null);
            
        }



        public void SendMessage(byte[] message)
        {
            IBasicProperties basicProperties = channel.CreateBasicProperties();
            basicProperties.SetPersistent(true);
            channel.BasicPublish("", QueueName, basicProperties, message);
        }


        public void SendMessageToEx(byte[] message, string ex, string key)
        {
            IBasicProperties basicProperties = channel.CreateBasicProperties();
            basicProperties.SetPersistent(true);
            
            channel.BasicPublish(exchange: ex,
                                 routingKey: key,
                                 basicProperties: null,
                                 body: message);
            
        }

        public void Dispose()
        {
            if (Connection != null)
                Connection.Close();

            if (channel != null)
                channel.Abort();

            if (connectionFactory != null)
                connectionFactory = null;
        }

    }
}
