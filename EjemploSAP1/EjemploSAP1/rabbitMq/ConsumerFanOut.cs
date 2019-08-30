using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using RabbitMQ.Client.Events;

namespace EjemploSAP1.rabbitMq
{
    class ConsumerFanOut
    {

        protected IModel channel;
        protected IConnection Connection;
        protected string queueName;

        ConnectionFactory connectionFactory;

        public bool isConsuming;

        public delegate void onReceiveMessage(byte[] message);
        public event onReceiveMessage onMessageReceived;

        public ConsumerFanOut(string hostName, string ex, string key)
        {
            
            connectionFactory = new ConnectionFactory();
            connectionFactory.HostName = hostName;
            
            connectionFactory.UserName = Program.myConf.userMsg;
            connectionFactory.Password = Program.myConf.passMsg;
            connectionFactory.VirtualHost = "/";

            Connection = connectionFactory.CreateConnection();

            channel = Connection.CreateModel();

            queueName = channel.QueueDeclare().QueueName;

            channel.QueueBind(queue: queueName,
                              exchange: ex,
                              routingKey: key);
            

        }

        private delegate void ConsumeDelegate();

        public void StartConsuming()
        {
            isConsuming = true;
            ConsumeDelegate c = new ConsumeDelegate(ConsumeFanOut);

            c.BeginInvoke(null, null);
        }


        RabbitMQ.Client.Events.BasicDeliverEventArgs e;
        QueueingBasicConsumer consumer;



        public void ConsumeFanOut()
        {

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);

                onMessageReceived(body);

            };


            channel.BasicConsume(queue: queueName,
                                 noAck: true,
                                 consumer: consumer);

        }
        
        public void Consume()
        {
            consumer = new QueueingBasicConsumer(channel);
            bool autoAck = false;
            String consumerTag = channel.BasicConsume("", autoAck, consumer);


            while (isConsuming)
            {
                try
                {

                    if (consumer != null)
                    {
                        e =
                            (RabbitMQ.Client.Events.BasicDeliverEventArgs)
                                consumer.Queue.Dequeue();



                        IBasicProperties props = e.BasicProperties;
                        byte[] body = e.Body;

                        string cade = System.Text.Encoding.UTF8.GetString(body);

                        try
                        {

                            System.Text.Encoding.UTF8.GetString(body);

                            onMessageReceived(body);

                            channel.BasicAck(e.DeliveryTag, false);

                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Error: en combertir el JSON");

                        }
                        
                    }

                }
                catch (OperationInterruptedException ex)
                {
                    break;
                }
            }

        }


        public void Dispose()
        {
            isConsuming = false;


            if (consumer != null)
            {
                consumer = null;
            }

            if (Connection != null)
                Connection.Close();

            if (channel != null)
                channel.Abort();

            if (connectionFactory != null)
                connectionFactory = null;

            if (Connection != null)
                Connection.Abort();

            if (e != null)
            {

                e = null;
            }

        }


    }
}
