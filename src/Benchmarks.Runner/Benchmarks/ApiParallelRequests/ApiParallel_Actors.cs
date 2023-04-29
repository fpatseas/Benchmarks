using Akka.Actor;
using Akka.Util;

namespace Benchmarks.Runner.Benchmarks.ApiParallelRequests
{
    internal sealed partial class ApiParallelBenchmarks
    {
        /// <summary>
        /// 
        /// </summary>
        public class ThrottleMessage
        {
            /// <summary>
            /// 
            /// </summary>
            public object Message { get; set; } = default!;

            /// <summary>
            /// 
            /// </summary>
            public IActorRef Target { get; set; } = default!;

            /// <summary>
            /// 
            /// </summary>
            public TaskCompletionSource<long> TaskCompletionSource { get; set; } = default!;

            /// <summary>
            /// 
            /// </summary>
            public long Response { get; set; }
        }

        public class ThrottlerActor : ReceiveActor
        {
            private Queue<ThrottleMessage> _queue;
            private int _maxConcurrent;
            private int _currentInFlight;

            public ThrottlerActor(int maxConcurrent)
            {
                _queue = new Queue<ThrottleMessage>();
                _maxConcurrent = maxConcurrent;
                _currentInFlight = 0;

                Receive<ThrottleMessage>(msg =>
                {
                    if (_currentInFlight < _maxConcurrent)
                    {
                        _currentInFlight++;
                        msg.Target.Tell(msg, Self);
                    }
                    else
                    {
                        _queue.Enqueue(msg);
                    }
                });

                Receive<ProcessedThrottleMessage>(msg =>
                {
                    if (_currentInFlight > 0)
                    {
                        _currentInFlight--;

                        // Set the result in the TaskCompletionSource
                        msg.TaskCompletionSource.SetResult(msg.Response);

                        if (_queue.Count > 0)
                        {
                            var nextMsg = _queue.Dequeue();
                            _currentInFlight++;
                            nextMsg.Target.Tell(nextMsg, Self);
                        }

                        Console.WriteLine(_currentInFlight);
                    }
                });
            }
        }


        public class WorkerActor : ReceiveActor
        {
            public WorkerActor()
            {
                ReceiveAsync<ThrottleMessage>(async msg =>
                {
                    // Make a request to the Web API and get the result
                    var result = await GetTestRequest(_httpClient);

                    // Set the response in the ThrottleMessage object
                    msg.Response = result;

                    // Notify the throttler that the request has been processed
                    Sender.Tell(new ProcessedThrottleMessage { TaskCompletionSource = msg.TaskCompletionSource, Response = msg.Response });
                });
            }
        }

        public class ProcessedThrottleMessage
        {
            public TaskCompletionSource<long> TaskCompletionSource { get; set; } = default!;
            public long Response { get; set; }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        //[Benchmark]
        //[BenchmarkCategory(ApiBenchmarks.ActorsWhenAllAsync)]
        //public async Task<long[]> UnlimitedActorsVersion() => await WhenAllVersionUsingAkka(-1);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        [BenchmarkCategory(ApiBenchmarks.ActorsWhenAllAsync)]
        public async Task<long[]> LimitedActorsVersion_10() => await WhenAllVersionUsingAkka(10);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        [BenchmarkCategory(ApiBenchmarks.ActorsWhenAllAsync)]
        public async Task<long[]> LimiteActorsVersion_20() => await WhenAllVersionUsingAkka(20);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        [BenchmarkCategory(ApiBenchmarks.ActorsWhenAllAsync)]
        public async Task<long[]> LimitedActorsVersion_50() => await WhenAllVersionUsingAkka(50);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        [BenchmarkCategory(ApiBenchmarks.ActorsWhenAllAsync)]
        public async Task<long[]> LimitedActorsVersion_100() => await WhenAllVersionUsingAkka(100);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxDegreeOfParallelism"></param>
        /// <returns></returns>
        public async Task<long[]> WhenAllVersionUsingAkka(int maxDegreeOfParallelism)
        {
            var batchSize = maxDegreeOfParallelism > 0 ? maxDegreeOfParallelism : short.MaxValue;
            var system = ActorSystem.Create("ThrottlerSystem");

            // Create a pool of worker actors
            var workerPool = new List<IActorRef>();
            for (int i = 0; i < batchSize; i++)
            {
                workerPool.Add(system.ActorOf(Props.Create(() => new WorkerActor()), $"worker-{i}"));
            }

            var throttler = system.ActorOf(Props.Create(() => new ThrottlerActor(batchSize)), "throttler");
            var tasks = new List<Task<long>>();

            for (int i = 0; i < TaskCount; i++)
            {
                var tcs = new TaskCompletionSource<long>();
                tasks.Add(tcs.Task);

                var worker = GetAvailableWorker(workerPool);
                throttler.Tell(new ThrottleMessage { Message = $"Request {i}", Target = worker, TaskCompletionSource = tcs });
            }

            long[] results = await Task.WhenAll(tasks);
            //Console.WriteLine($"All requests completed. Results: {string.Join(", ", results)}");

            PlotBenchmarkResults(results, $"WhenAllAkkaVersion - {maxDegreeOfParallelism}");

            return results;
        }

        private IActorRef GetAvailableWorker(IList<IActorRef> workerPool)
        {
            // Select a worker actor from the pool using round-robin or any other load-balancing strategy
            return workerPool[ThreadLocalRandom.Current.Next(workerPool.Count)];
        }
    }
}
