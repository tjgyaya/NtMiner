﻿using NTMiner.Core.MinerServer;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;

namespace NTMiner.Core.Mq.Senders.Impl {
    public class MinerClientMqSender : IMinerClientMqSender {
        private readonly IMq _mq;
        public MinerClientMqSender(IMq mq) {
            _mq = mq;
        }

        public void SendSpeeds(ClientIdIp[] clientIdIps) {
            if (clientIdIps == null || clientIdIps.Length == 0) {
                return;
            }
            var basicProperties = CreateNonePersistentBasicProperties();
            _mq.BasicPublish(
                routingKey: MqKeyword.SpeedsRoutingKey,
                basicProperties: basicProperties,
                body: MinerClientMqBodyUtil.GetClientIdIpsMqSendBody(clientIdIps));
        }

        public void SendMinerClientWsClosed(Guid clientId) {
            if (clientId == Guid.Empty) {
                return;
            }
            var basicProperties = CreateNonePersistentBasicProperties();
            _mq.BasicPublish(
                routingKey: MqKeyword.MinerClientWsClosedRoutingKey,
                basicProperties: basicProperties,
                body: MinerClientMqBodyUtil.GetClientIdMqSendBody(clientId));
        }

        public void SendMinerClientsWsBreathed(Guid[] clientIds) {
            if (clientIds == null || clientIds.Length == 0) {
                return;
            }
            var basicProperties = CreateNonePersistentBasicProperties();
            _mq.BasicPublish(
                routingKey: MqKeyword.MinerClientsWsBreathedRoutingKey,
                basicProperties: basicProperties,
                body: MinerClientMqBodyUtil.GetClientIdsMqSendBody(clientIds));
        }

        public void SendMinerSignsSeted(MinerSign[] minerSigns) {
            if (minerSigns == null || minerSigns.Length == 0) {
                return;
            }
            var basicProperties = CreateNonePersistentBasicProperties();
            _mq.BasicPublish(
                routingKey: MqKeyword.MinerSignsSetedRoutingKey,
                basicProperties: basicProperties,
                body: MinerClientMqBodyUtil.GetMinerSignsMqSendBody(minerSigns));
        }

        public void SendQueryClientsForWs(
            Guid studioId, 
            string sessionId, 
            QueryClientsForWsRequest request) {
            if (string.IsNullOrEmpty(sessionId) || request == null || string.IsNullOrEmpty(request.LoginName)) {
                return;
            }
            var basicProperties = CreateNonePersistentWsBasicProperties(request.LoginName, studioId, sessionId);
            _mq.BasicPublish(
                routingKey: MqKeyword.QueryClientsForWsRoutingKey,
                basicProperties: basicProperties,
                body: MinerClientMqBodyUtil.GetQueryClientsForWsMqSendBody(request));
        }

        private IBasicProperties CreateNonePersistentBasicProperties() {
            var basicProperties = _mq.CreateBasicProperties();
            basicProperties.Persistent = false;// 非持久化的
            basicProperties.Expiration = MqKeyword.Expiration36sec;

            return basicProperties;
        }

        private IBasicProperties CreateNonePersistentWsBasicProperties(string loginName, Guid studioId, string sessionId) {
            var basicProperties = _mq.CreateBasicProperties();
            basicProperties.Persistent = false;// 非持久化的
            basicProperties.Expiration = MqKeyword.Expiration36sec;
            basicProperties.Headers = new Dictionary<string, object> {
                [MqKeyword.LoginNameHeaderName] = loginName,
                [MqKeyword.StudioIdHeaderName] = studioId.ToString(),
                [MqKeyword.SessionIdHeaderName] = sessionId
            };

            return basicProperties;
        }
    }
}
