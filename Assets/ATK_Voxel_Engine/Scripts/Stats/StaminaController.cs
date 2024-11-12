using System;
using UnityEngine;
using ATKVoxelEngine;

public class StaminaController : ITickable
{
    public TickType TickType => TickType.UPDATE;

    [SerializeField] StaminaStats_SO _stats;
    public StaminaStats_SO Stats => _stats;

    public float CurrentStamina { get; private set; }

    float _regenDelayTimer = 0.0f;
    float _lastTickTime = 0.0f;
    bool _isRegenDelay = false;

    public Action<float, float> OnStaminaChanged;

    public StaminaController(StaminaStats_SO stats)
    {
        _stats = stats;
        CurrentStamina = stats.MaxStamina;
        Register();
    }

    ~StaminaController()
    {
        UnRegister();
    }

    public void Register()
    {
        TickRateManager.OnUpdate += Tick;
    }

    public void UnRegister()
    {
        TickRateManager.OnUpdate -= Tick;
    }

    protected void Tick(float deltaTime)
    {
        if (CurrentStamina == _stats.MaxStamina) return;

        if (_regenDelayTimer < _stats.RegenDelay && _isRegenDelay)
        {
            _regenDelayTimer += deltaTime;
            return;
        }
        else if (_isRegenDelay)
        {
            _lastTickTime = Time.time;
            _isRegenDelay = false;
        }

        if (Time.time - _lastTickTime >= _stats.TickRate)
        {
            float delta = Time.time - _lastTickTime;
            CurrentStamina = Mathf.Clamp((delta * (_stats.RegenPerTick / _stats.TickRate)) + CurrentStamina , 0, _stats.MaxStamina);
            OnStaminaChanged?.Invoke(CurrentStamina, _stats.MaxStamina);
            _lastTickTime = Time.time;
        }
    }

    public bool TryConsume(float consumtion)
    {
        float newStamina = Mathf.Clamp(CurrentStamina - Mathf.Abs(consumtion), 0, _stats.MaxStamina);
        if (newStamina > 0)
        {
            CurrentStamina = newStamina;
            _regenDelayTimer = 0;
            _isRegenDelay = true;
            OnStaminaChanged?.Invoke(CurrentStamina, _stats.MaxStamina);
            return true;
        }

        return false;
    }

    void ITickable.Register()
    {
        throw new NotImplementedException();
    }

    void ITickable.UnRegister()
    {
        throw new NotImplementedException();
    }

    void ITickable.Tick(float deltaTime)
    {
        throw new NotImplementedException();
    }
}
