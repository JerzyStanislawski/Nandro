using DotNano.RpcApi.Responses;
using FakeItEasy;
using FluentAssertions;
using Nandro.Nano;
using Nandro.TransactionMonitors;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Numerics;
using System.Text;
using Xunit;

namespace Nandro.Tests
{
    public class TransactionMonitorTests
    {
        const string _socketResponse = @"{
  ""topic"": ""confirmation"",
  ""time"": ""1618148121109"",
  ""message"": {
    ""account"": ""nano_3rihs8dcjne3tjummjeotsz8r5aj319huj8xfm6rj16pfyjjri3duzxhionx"",
    ""amount"": ""10000000000000000000000000"",
    ""hash"": ""7D91514BA4FC1FA6CC1AF547A47AAFB3CF73C367FF44B8E9F63A9B0CF857DB3A"",
    ""confirmation_type"": ""active_quorum"",
    ""block"": {
      ""type"": ""state"",
      ""account"": ""nano_3rihs8dcjne3tjummjeotsz8r5aj319huj8xfm6rj16pfyjjri3duzxhionx"",
      ""previous"": ""5C446A9D4EF664580DF856F79F9BB2C1ECA59896E63388E1A20BBABD2398EA11"",
      ""representative"": ""nano_1natrium1o3z5519ifou7xii8crpxpk8y65qmkih8e8bpsjri651oza8imdd"",
      ""balance"": ""18887720000000000000000000000000"",
      ""link"": ""8AD883D7DE7C3B15B26BC69400AC5DC7CCE4ABD973DD9876420E1A361C3C2EFC"",
      ""link_as_account"": ""nano_34prihdxwz3u4ps8qjnn14p7ujyewkoxkwyxm3u665it8rg5rdqw84qrypzk"",
      ""signature"": ""4F33BF98F10B3EAC5CB99ADED81C74A94076F7D28B3D5CC3480792452C9B78B35066BB93F23AA93ED394F6081DD7994C99195B6F4C606751C5840A44378A0303"",
      ""work"": ""7a9dd9c91430b639"",
      ""subtype"": ""send""
    }
  }
}
";

        private TransactionMonitor Initialize(INanoClient nanoClient, IWebSocket socket)
        {
            var config = new Configuration() { TransactionTimeoutSec = 3 };
            var rpcTransactionMonitor = new RpcTransactionMonitor(nanoClient, config);
            var socketTransactionMonitor = new SocketTransactionMonitor(new NanoSocketClient(socket, config), config);
            var transactionMonitor = new TransactionMonitor(socketTransactionMonitor, rpcTransactionMonitor);

            return transactionMonitor;
        }

        private void MockSocketResponse(IWebSocket socket, string socketMessage)
        {
            var jsonAckResponse = "{\"ack\": \"subscribe\", \"id\": \"\" }";
            var ackResponse = new WebSocketReceiveResult(jsonAckResponse.Length, WebSocketMessageType.Text, true);

            if (socketMessage != null)
            {
                var socketMessageResponse = new WebSocketReceiveResult(socketMessage.Length, WebSocketMessageType.Text, true);

                A.CallTo(() => socket.Receive(A<ArraySegment<byte>>._, A<int>._))
                    .Invokes((ArraySegment<byte> output, int _) => Encoding.UTF8.GetBytes(jsonAckResponse, 0, jsonAckResponse.Length, output.Array, 0))
                    .Returns(ackResponse)
                    .Once()
                    .Then
                    .Invokes((ArraySegment<byte> output, int _) => Encoding.UTF8.GetBytes(socketMessage, 0, socketMessage.Length, output.Array, 0))
                    .Returns(socketMessageResponse);
            }
            else
            {
                A.CallTo(() => socket.Receive(A<ArraySegment<byte>>._, A<int>._))
                    .Invokes((ArraySegment<byte> output, int _) => Encoding.UTF8.GetBytes(jsonAckResponse, 0, jsonAckResponse.Length, output.Array, 0))
                    .Returns(ackResponse)
                    .Once()
                    .Then
                    .Throws(new AggregateException(new OperationCanceledException()));
            }
            A.CallTo(() => socket.State).Returns(WebSocketState.Open);
        }

        [Fact]
        public void TransactionMonitor_ShouldVerifyTransactionWithSocket_WhenConfirmationReceived()
        {
            var socket = A.Fake<IWebSocket>();
            MockSocketResponse(socket, _socketResponse);

            var transactionMonitor = Initialize(A.Fake<INanoClient>(), socket);
            var result = transactionMonitor.Verify("nano_34prihdxwz3u4ps8qjnn14p7ujyewkoxkwyxm3u665it8rg5rdqw84qrypzk", BigInteger.Parse("10000000000000000000000000"));

            result.Should().BeTrue();
        }

        [Fact]
        public void TransactionMonitor_ShouldFailToVerifyTransactionWithSocket_WhenConfirmationReceivedWithWrongAmount()
        {
            var socket = A.Fake<IWebSocket>();
            MockSocketResponse(socket, _socketResponse);

            var transactionMonitor = Initialize(A.Fake<INanoClient>(), socket);
            var result = transactionMonitor.Verify("nano_34prihdxwz3u4ps8qjnn14p7ujyewkoxkwyxm3u665it8rg5rdqw84qrypzk", BigInteger.Parse("2000000000000000000000000"));

            result.Should().BeFalse();
        }

        [Fact]
        public void TransactionMonitor_ShouldFailToVerifyTransactionWithSocket_WhenNoConfirmationReceived()
        {
            var socket = A.Fake<IWebSocket>();
            MockSocketResponse(socket, null);

            var transactionMonitor = Initialize(A.Fake<INanoClient>(), socket);
            var result = transactionMonitor.Verify("nano_34prihdxwz3u4ps8qjnn14p7ujyewkoxkwyxm3u665it8rg5rdqw84qrypzk", BigInteger.Parse("10000000000000000000000000000"));

            result.Should().BeFalse();
        }

        [Fact]
        public void TransactionMonitor_ShouldVerifyTransactionWithRpcClient_WhenBlockReceived()
        {
            const string account = "nano_34prihdxwz3u4ps8qjnn14p7ujyewkoxkwyxm3u665it8rg5rdqw84qrypzk";

            var socket = A.Fake<IWebSocket>();
            A.CallTo(() => socket.State).Returns(WebSocketState.Closed);

            var nanoClient = A.Fake<INanoClient>();
            A.CallTo(() => nanoClient.GetFrontier(account)).Returns("DB510C1E22C8DDEAC7C74D6079D5422DADC0785E54A62FFEC9EF20AD5F164AD6");
            A.CallTo(() => nanoClient.GetLatestTransaction(account)).Returns(new AccountHistoryHistory
            {
                Account = "nano_3ganr3ikbci349euims3srpxe39iz1cyos45yhypf49mwx51paxa9qjzypc7",
                Amount = BigInteger.Parse("10000000000000000000000000"),
                Hash = "F544E6C8866616C9EB517DB12D559F39028A9EE141595FB99EA6373B923D8013",
                Type = "receive"
            });

            var transactionMonitor = Initialize(nanoClient, socket);
            var result = transactionMonitor.Verify(account, BigInteger.Parse("10000000000000000000000000"));

            result.Should().BeTrue();
        }

        [Fact]
        public void TransactionMonitor_ShouldFailToVerifyTransactionWithRpcClient_WhenBlocksReceivedWithWrongAmount()
        {
            const string account = "nano_34prihdxwz3u4ps8qjnn14p7ujyewkoxkwyxm3u665it8rg5rdqw84qrypzk";

            var socket = A.Fake<IWebSocket>();
            A.CallTo(() => socket.State).Returns(WebSocketState.Closed);

            var nanoClient = A.Fake<INanoClient>();
            A.CallTo(() => nanoClient.GetFrontier(account)).Returns("DB510C1E22C8DDEAC7C74D6079D5422DADC0785E54A62FFEC9EF20AD5F164AD6");
            A.CallTo(() => nanoClient.GetLatestTransaction(account)).Returns(new AccountHistoryHistory
            {
                Account = "nano_3ganr3ikbci349euims3srpxe39iz1cyos45yhypf49mwx51paxa9qjzypc7",
                Amount = new BigInteger(12345678),
                Hash = "F544E6C8866616C9EB517DB12D559F39028A9EE141595FB99EA6373B923D8013",
                Type = "receive"
            });

            var transactionMonitor = Initialize(nanoClient, socket);
            var result = transactionMonitor.Verify(account, BigInteger.Parse("10000000000000000000000000"));

            result.Should().BeFalse();
        }

        [Fact]
        public void TransactionMonitor_ShouldFailToVerifyTransactionWithRpcClient_WhenBlocksReceivedWithCorrectAmountButWrongType()
        {
            const string account = "nano_34prihdxwz3u4ps8qjnn14p7ujyewkoxkwyxm3u665it8rg5rdqw84qrypzk";

            var socket = A.Fake<IWebSocket>();
            A.CallTo(() => socket.State).Returns(WebSocketState.Closed);

            var nanoClient = A.Fake<INanoClient>();
            A.CallTo(() => nanoClient.GetFrontier(account)).Returns("DB510C1E22C8DDEAC7C74D6079D5422DADC0785E54A62FFEC9EF20AD5F164AD6");
            A.CallTo(() => nanoClient.GetLatestTransaction(account)).Returns(new AccountHistoryHistory
            {
                Account = "nano_3ganr3ikbci349euims3srpxe39iz1cyos45yhypf49mwx51paxa9qjzypc7",
                Amount = BigInteger.Parse("10000000000000000000000000"),
                Hash = "F544E6C8866616C9EB517DB12D559F39028A9EE141595FB99EA6373B923D8013",
                Type = "send"
            });

            var transactionMonitor = Initialize(nanoClient, socket);
            var result = transactionMonitor.Verify(account, BigInteger.Parse("10000000000000000000000000"));

            result.Should().BeFalse();
        }

        [Fact]
        public void TransactionMonitor_ShouldFailToVerifyTransactionWithRpcClient_WhenNoNewReceiveBlocksAndNoNewPendingTxs()
        {
            const string account = "nano_34prihdxwz3u4ps8qjnn14p7ujyewkoxkwyxm3u665it8rg5rdqw84qrypzk";
            const string frontierHash = "DB510C1E22C8DDEAC7C74D6079D5422DADC0785E54A62FFEC9EF20AD5F164AD6";
            var amount = BigInteger.Parse("10000000000000000000000000");

            var socket = A.Fake<IWebSocket>();
            A.CallTo(() => socket.State).Returns(WebSocketState.Closed);

            var nanoClient = A.Fake<INanoClient>();
            A.CallTo(() => nanoClient.GetFrontier(account)).Returns(frontierHash);
            A.CallTo(() => nanoClient.GetLatestTransaction(account)).Returns(new AccountHistoryHistory
            {
                Account = "nano_3ganr3ikbci349euims3srpxe39iz1cyos45yhypf49mwx51paxa9qjzypc7",
                Amount = BigInteger.Parse("10000000000000000000000000"),
                Hash = frontierHash,
                Type = "receive"
            });
            A.CallTo(() => nanoClient.GetPendingTxs(account))
                .Returns(new Dictionary<string, BigInteger>
                {
                    { "001341B8A6EF9D7555458773C2B19C94CBFB835A41DE6853B10F780DF9EA8A05", amount },
                    { "00F95A007E4D21F24D49C0D764CD1BB56FF581A0B2B2A4763D33DC97C9341E03", amount },
                    { "018F35F0A18E837E30D927410C98E2E00DADE4CF702F1B20AA520AF341385685", amount }
                })
                .Once()
                .Then
                .Returns(new Dictionary<string, BigInteger>
                {
                    { "001341B8A6EF9D7555458773C2B19C94CBFB835A41DE6853B10F780DF9EA8A05", amount },
                    { "00F95A007E4D21F24D49C0D764CD1BB56FF581A0B2B2A4763D33DC97C9341E03", amount },
                    { "018F35F0A18E837E30D927410C98E2E00DADE4CF702F1B20AA520AF341385685", amount }
                });

            var transactionMonitor = Initialize(nanoClient, socket);
            var result = transactionMonitor.Verify(account, amount);

            result.Should().BeFalse();
        }

        [Fact]
        public void TransactionMonitor_ShouldVerifyTransactionWithRpcClient_WhenNewPendingTxDetectedOnNewAccount()
        {
            const string account = "nano_34prihdxwz3u4ps8qjnn14p7ujyewkoxkwyxm3u665it8rg5rdqw84qrypzk";

            var socket = A.Fake<IWebSocket>();
            A.CallTo(() => socket.State).Returns(WebSocketState.Closed);

            var nanoClient = A.Fake<INanoClient>();
            A.CallTo(() => nanoClient.GetFrontier(account)).Returns(null);
            A.CallTo(() => nanoClient.GetLatestTransaction(account)).Returns(null);
            A.CallTo(() => nanoClient.GetPendingTxs(account))
                .Returns(new Dictionary<string, BigInteger>())
                .Once()
                .Then
                .Returns(new Dictionary<string, BigInteger>
                {
                    { "001341B8A6EF9D7555458773C2B19C94CBFB835A41DE6853B10F780DF9EA8A05", BigInteger.Parse("10000000000000000000000000") }
                });

            var transactionMonitor = Initialize(nanoClient, socket);
            var result = transactionMonitor.Verify(account, BigInteger.Parse("10000000000000000000000000"));

            result.Should().BeTrue();
        }

        [Fact]
        public void TransactionMonitor_ShouldVerifyTransactionWithRpcClient_WhenNewPendingTxDetected()
        {
            const string account = "nano_34prihdxwz3u4ps8qjnn14p7ujyewkoxkwyxm3u665it8rg5rdqw84qrypzk";
            var amount = BigInteger.Parse("10000000000000000000000000");

            var socket = A.Fake<IWebSocket>();
            A.CallTo(() => socket.State).Returns(WebSocketState.Closed);

            var nanoClient = A.Fake<INanoClient>();
            A.CallTo(() => nanoClient.GetFrontier(account)).Returns(null);
            A.CallTo(() => nanoClient.GetLatestTransaction(account)).Returns(null);
            A.CallTo(() => nanoClient.GetPendingTxs(account))
                .Returns(new Dictionary<string, BigInteger>
                {
                    { "001341B8A6EF9D7555458773C2B19C94CBFB835A41DE6853B10F780DF9EA8A05", amount },
                    { "00F95A007E4D21F24D49C0D764CD1BB56FF581A0B2B2A4763D33DC97C9341E03", amount },
                    { "018F35F0A18E837E30D927410C98E2E00DADE4CF702F1B20AA520AF341385685", amount }
                })
                .Once()
                .Then
                .Returns(new Dictionary<string, BigInteger>
                {
                    { "001341B8A6EF9D7555458773C2B19C94CBFB835A41DE6853B10F780DF9EA8A05", amount },
                    { "00F95A007E4D21F24D49C0D764CD1BB56FF581A0B2B2A4763D33DC97C9341E03", amount },
                    { "018F35F0A18E837E30D927410C98E2E00DADE4CF702F1B20AA520AF341385685", amount },
                    { "06C45EBF775A1438339D6176827BD9E68053E524FAA92FD5A6A8CC7DAD344527", amount }
                });

            var transactionMonitor = Initialize(nanoClient, socket);
            var result = transactionMonitor.Verify(account, amount);

            result.Should().BeTrue();
        }

        [Fact]
        public void TransactionMonitor_ShouldFailToVerifyTransactionWithRpcClient_WhenNewPendingTxWithWrongAmountDetected()
        {
            const string account = "nano_34prihdxwz3u4ps8qjnn14p7ujyewkoxkwyxm3u665it8rg5rdqw84qrypzk";
            var amount = BigInteger.Parse("10000000000000000000000000");

            var socket = A.Fake<IWebSocket>();
            A.CallTo(() => socket.State).Returns(WebSocketState.Closed);

            var nanoClient = A.Fake<INanoClient>();
            A.CallTo(() => nanoClient.GetFrontier(account)).Returns(null);
            A.CallTo(() => nanoClient.GetLatestTransaction(account)).Returns(null);
            A.CallTo(() => nanoClient.GetPendingTxs(account))
                .Returns(new Dictionary<string, BigInteger>
                {
                    { "001341B8A6EF9D7555458773C2B19C94CBFB835A41DE6853B10F780DF9EA8A05", amount },
                    { "00F95A007E4D21F24D49C0D764CD1BB56FF581A0B2B2A4763D33DC97C9341E03", amount },
                    { "018F35F0A18E837E30D927410C98E2E00DADE4CF702F1B20AA520AF341385685", amount }
                })
                .Once()
                .Then
                .Returns(new Dictionary<string, BigInteger>
                {
                    { "001341B8A6EF9D7555458773C2B19C94CBFB835A41DE6853B10F780DF9EA8A05", amount },
                    { "00F95A007E4D21F24D49C0D764CD1BB56FF581A0B2B2A4763D33DC97C9341E03", amount },
                    { "018F35F0A18E837E30D927410C98E2E00DADE4CF702F1B20AA520AF341385685", amount },
                    { "06C45EBF775A1438339D6176827BD9E68053E524FAA92FD5A6A8CC7DAD344527", new BigInteger(1231243245) }
                });

            var transactionMonitor = Initialize(nanoClient, socket);
            var result = transactionMonitor.Verify(account, amount);

            result.Should().BeFalse();
        }
    }
}
