using System.Collections.Generic;
using kOS.Safe;

namespace kOS
{
    public class TransferManager : IUpdateObserver
    {
        private readonly SharedObjects shared;
        private readonly List<ResourceTransferValue> transfers;

        public TransferManager(SharedObjects shared)
        {
            this.shared = shared;
            transfers = new List<ResourceTransferValue>();
            shared.UpdateHandler.AddObserver(this);
        }

        public List<ResourceTransferValue> Transfers
        {
            get { return transfers; }
        }

        public ResourceTransferValue CreateTransfer(PartResourceDefinition resourceInfo, object transferTo, object transferFrom, double amount)
        {
            var toReturn = new ResourceTransferValue(resourceInfo, transferTo, transferFrom, amount);
            transfers.Add(toReturn);
            return toReturn;
        }

        public ResourceTransferValue CreateTransfer(PartResourceDefinition resourceInfo, object transferTo, object transferFrom)
        {
            var toReturn = new ResourceTransferValue(resourceInfo, transferTo, transferFrom);
            transfers.Add(toReturn);
            return toReturn;
        }

        public void Dispose()
        {
            shared.UpdateHandler.RemoveObserver(this);
        }

        public void Update(double deltaTime)
        {
            foreach (var transfer in transfers)
            {
                
            }
        }
    }
}