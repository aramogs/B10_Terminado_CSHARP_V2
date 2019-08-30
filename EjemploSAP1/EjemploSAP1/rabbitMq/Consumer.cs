using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace EjemploSAP1.rabbitMq
{
    class Consumer
    {

        protected IModel Model;
        protected IConnection Connection;
        protected string QueueName;

        ConnectionFactory connectionFactory;

        public bool isConsuming;
        public delegate void onReceiveMessage(byte[] message);
        public event onReceiveMessage onMessageReceived;


        public Consumer(string hostName, string queueName)
        {
            QueueName = queueName;
            connectionFactory = new ConnectionFactory();
            connectionFactory.HostName = hostName;
            
            connectionFactory.UserName = Program.myConf.userMsg;
            connectionFactory.Password = Program.myConf.passMsg;

            connectionFactory.VirtualHost = "/";

            Connection = connectionFactory.CreateConnection();

            Model = Connection.CreateModel();
            Model.BasicQos(0, 1, false);
            bool durable = true;
            Model.QueueDeclare(QueueName, durable, false, false, null);

        }

        private delegate void ConsumeDelegate();

        public void StartConsuming()
        {
            isConsuming = true;
            ConsumeDelegate c = new ConsumeDelegate(Consume);
            c.BeginInvoke(null, null);
        }


        RabbitMQ.Client.Events.BasicDeliverEventArgs e;
        QueueingBasicConsumer consumer;

        public void Consume()
        {
            consumer = new QueueingBasicConsumer(Model);
            bool autoAck = false;
            String consumerTag = Model.BasicConsume(QueueName, autoAck, consumer);


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
                        Console.WriteLine("cade: " + cade);

                        try
                        {

                            System.Text.Encoding.UTF8.GetString(body);


                            onMessageReceived(body);

                            Model.BasicAck(e.DeliveryTag, false);

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

            if (Model != null)
                Model.Abort();

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
