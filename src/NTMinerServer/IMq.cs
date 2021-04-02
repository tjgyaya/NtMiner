﻿using RabbitMQ.Client;

namespace NTMiner {
    public interface IMq {
        /// <summary>
        /// 返回的对象已赋值MessageId和AppId
        /// </summary>
        /// <returns></returns>
        IBasicProperties CreateBasicProperties();
        void BasicPublish(string exchange, string routingKey, IBasicProperties basicProperties, byte[] body);
    }
}