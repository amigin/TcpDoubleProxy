using System;
using CommonPart;
using Xunit;

namespace TestsProject
{
    public class TestTechnicalSerializers
    {
        [Fact]
        public void TestConnected()
        {

            
            var serverTechServer = new TechSocketMock();
            var clientTechServer = new TechSocketMock();


            serverTechServer.BindToSocket(clientTechServer);
            clientTechServer.BindToSocket(clientTechServer);
            
            
            serverTechServer.RegisterOtherStream(clientTechServer);
            clientTechServer.RegisterOtherStream(serverTechServer);


            serverTechServer.NotifyThatSocketConnected(5678).AsTask().Wait();
            
            serverTechServer.PushDataViaSocket();


            clientTechServer.TechSocketConnection.HandleServiceSocketReadAsync().AsTask().Wait();
            
            serverTechServer.SendDataAsync(5678, new ReadOnlyMemory<byte>(new byte[]{1,2,3,4,5})).AsTask().Wait();
            serverTechServer.PushDataViaSocket();
            clientTechServer.TechSocketConnection.HandleServiceSocketReadAsync().AsTask().Wait();
            
            
           Assert.Equal("Connected: 5678", clientTechServer.RegisteredEvents[0]);
           Assert.Equal("HasData: 5678; Data: 0102030405", clientTechServer.RegisteredEvents[1]);
 
        }
        
        
    }
}