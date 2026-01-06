using System;
using System.Threading;
using System.Threading.Tasks;

namespace ScientistCardGame.Services
{
    public class TurnTimerService
    {
        private int _timeLimit; // seconds
        private int _timeRemaining;
        private bool _isRunning;
        private CancellationTokenSource _cancellationTokenSource;

        public event Action<int> OnTimerTick; // Fires every second with time remaining
        public event Action OnTimerExpired; // Fires when timer hits 0

        public bool IsEnabled { get; set; }
        public int TimeLimit
        {
            get => _timeLimit;
            set => _timeLimit = value;
        }

        public int TimeRemaining => _timeRemaining;
        public bool IsRunning => _isRunning;

        public TurnTimerService(int timeLimit = 60)
        {
            _timeLimit = timeLimit;
            _timeRemaining = timeLimit;
            _isRunning = false;
            IsEnabled = false;
        }

        // Start the timer
        public void StartTimer()
        {
            if (!IsEnabled) return;

            if (_isRunning)
                StopTimer();

            _timeRemaining = _timeLimit;
            _isRunning = true;
            _cancellationTokenSource = new CancellationTokenSource();

            Task.Run(async () => await RunTimer(_cancellationTokenSource.Token));
        }

        // Stop the timer
        public void StopTimer()
        {
            _isRunning = false;
            _cancellationTokenSource?.Cancel();
        }

        // Reset timer to full time
        public void ResetTimer()
        {
            _timeRemaining = _timeLimit;
        }

        // Main timer loop
        private async Task RunTimer(CancellationToken cancellationToken)
        {
            while (_isRunning && _timeRemaining > 0 && !cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000, cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                    break;

                _timeRemaining--;

                // Notify subscribers
                OnTimerTick?.Invoke(_timeRemaining);

                if (_timeRemaining <= 0)
                {
                    _isRunning = false;
                    OnTimerExpired?.Invoke();
                    break;
                }
            }
        }

        // Get formatted time string
        public string GetTimeString()
        {
            int minutes = _timeRemaining / 60;
            int seconds = _timeRemaining % 60;
            return $"{minutes:00}:{seconds:00}";
        }

        // Change time limit (30s, 60s, 90s, unlimited)
        public void SetTimeLimit(int seconds)
        {
            _timeLimit = seconds;
            _timeRemaining = seconds;
        }
    }
}