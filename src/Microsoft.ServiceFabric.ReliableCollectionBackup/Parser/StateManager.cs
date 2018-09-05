using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using Microsoft.ServiceFabric.Replicator;
using Microsoft.ServiceFabric.Tools.ReliabilitySimulator;

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.Parser
{
    class StateManager : IReliableStateManager
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="reliabilitySimulator"></param>
        public StateManager(ReliabilitySimulator.ReliabilitySimulator reliabilitySimulator)
        {
            this.reliabilitySimulator = reliabilitySimulator;
        }

        public ITransaction CreateTransaction()
        {
            return this.replicator.CreateTransaction();
        }

        public IAsyncEnumerator<IReliableState> GetAsyncEnumerator()
        {
            return this.replicator.CreateAsyncEnumerable(true, false).GetAsyncEnumerator();
        }

        public async Task<T> GetOrAddAsync<T>(ITransaction tx, Uri name, TimeSpan timeout) where T : IReliableState
        {
            var result = await this.replicator.GetOrAddStateProviderAsync(
                (Transaction)tx,
                name,
                (Uri n) =>
                {
                    var passedType = typeof(T);
                    var needToWrap = !passedType.GetTypeInfo().IsClass;
                    var concreteType = needToWrap
                        ? this._typeCache.GetConcreteType(passedType)
                        : passedType;

                    return (IStateProvider2)Activator.CreateInstance(concreteType);
                },
                timeout,
                CancellationToken.None).ConfigureAwait(false);

            try
            {
                return (T)result.Value;
            }
            catch (InvalidCastException)
            {
                // add log
                throw;
            }
        }

        public Task<T> GetOrAddAsync<T>(ITransaction tx, Uri name) where T : IReliableState
        {
            return this.GetOrAddAsync<T>(tx, name, DefaultTimeout);
        }

        public async Task<T> GetOrAddAsync<T>(Uri name, TimeSpan timeout) where T : IReliableState
        {
            try
            {
                using (var tx = this.CreateTransaction())
                {
                    var result = await ((IReliableStateManager)this).GetOrAddAsync<T>(tx, name, timeout).ConfigureAwait(false);
                    await tx.CommitAsync().ConfigureAwait(false);
                    return result;
                }
            }
            catch (Exception)
            {
                // add log..
                throw;
            }
        }

        public Task<T> GetOrAddAsync<T>(Uri name) where T : IReliableState
        {
            return this.GetOrAddAsync<T>(name, DefaultTimeout);
        }

        public Task<T> GetOrAddAsync<T>(ITransaction tx, string name, TimeSpan timeout) where T : IReliableState
        {
            return this.GetOrAddAsync<T>(tx, new Uri(name), timeout);
        }

        public Task<T> GetOrAddAsync<T>(ITransaction tx, string name) where T : IReliableState
        {
            return this.GetOrAddAsync<T>(tx, new Uri(name));
        }

        public Task<T> GetOrAddAsync<T>(string name, TimeSpan timeout) where T : IReliableState
        {
            return this.GetOrAddAsync<T>(new Uri(name), timeout);
        }

        public Task<T> GetOrAddAsync<T>(string name) where T : IReliableState
        {
            return this.GetOrAddAsync<T>(new Uri(name));
        }

        public Task RemoveAsync(ITransaction tx, Uri name, TimeSpan timeout)
        {
            return this.replicator.RemoveStateProviderAsync((Transaction) tx, name, timeout, CancellationToken.None);
        }

        public Task RemoveAsync(ITransaction tx, Uri name)
        {
            return this.RemoveAsync(tx, name, DefaultTimeout);
        }

        async public Task RemoveAsync(Uri name, TimeSpan timeout)
        {
            using (var tx = this.CreateTransaction())
            {
                await this.RemoveAsync(tx, name, timeout).ConfigureAwait(false);
                await tx.CommitAsync().ConfigureAwait(false);
            }
        }

        public Task RemoveAsync(Uri name)
        {
            return this.RemoveAsync(name, DefaultTimeout);
        }

        public Task RemoveAsync(ITransaction tx, string name, TimeSpan timeout)
        {
            return this.RemoveAsync(tx, new Uri(name), DefaultTimeout);
        }

        public Task RemoveAsync(ITransaction tx, string name)
        {
            return this.RemoveAsync(tx, name, DefaultTimeout);
        }

        public Task RemoveAsync(string name, TimeSpan timeout)
        {
            return this.RemoveAsync(new Uri(name), timeout);
        }

        public Task RemoveAsync(string name)
        {
            return this.RemoveAsync(name, DefaultTimeout);
        }

        public Task<ConditionalValue<T>> TryGetAsync<T>(Uri name) where T : IReliableState
        {
            var result = this.replicator.TryGetStateProvider(name);
            if (result.HasValue)
            {
                return Task.FromResult(new ConditionalValue<T>(true, (T)result.Value));
            }

            return Task.FromResult(new ConditionalValue<T>());
        }

        public Task<ConditionalValue<T>> TryGetAsync<T>(string name) where T : IReliableState
        {
            var uriName = "urn:" + Uri.EscapeDataString(name);
            return this.TryGetAsync<T>(new Uri(uriName));
        }

        public bool TryAddStateSerializer<T>(IStateSerializer<T> stateSerializer)
        {
            return this.replicator.TryAddStateSerializer<T>(stateSerializer);
        }

        public event EventHandler<NotifyTransactionChangedEventArgs> TransactionChanged
        {
            add { this.replicator.TransactionChanged += value; }
            remove { this.replicator.TransactionChanged += value; }
        }

        public event EventHandler<NotifyStateManagerChangedEventArgs> StateManagerChanged
        {
            add { this.replicator.StateManagerChanged += value; }
            remove { this.replicator.StateManagerChanged -= value; }
        }

        private TransactionalReplicator replicator
        {
            get { return this.reliabilitySimulator.GetTransactionalReplicator(); }
        }

        private ReliabilitySimulator.ReliabilitySimulator reliabilitySimulator;
        private readonly StateManagerTypeCache _typeCache = new StateManagerTypeCache();
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(4);
    }
}
