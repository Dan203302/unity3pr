using UnityEngine;
using System;
using System.Collections.Generic;

public class BalanceTester : MonoBehaviour
{
    [SerializeField] private BalanceConfig balanceConfig;

    [Header("Тестовые параметры")]
    [SerializeField] private int testLevels = 10;
    [SerializeField] private float collectionRate = 5f;

    [Serializable]
    public class BalanceTestResult
    {
        public int level;
        public int abilityIndex;
        public float timeToAfford;
        public float dps;
        public float totalCost;
    }

    private List<BalanceTestResult> testResults = new List<BalanceTestResult>();

    void Start()
    {
        if (balanceConfig == null) { Debug.LogError("[BalanceTester] BalanceConfig не назначен!"); return; }
        RunBalanceTests();
        AnalyzeResults();
        ExportToCSV();
    }

    void RunBalanceTests()
    {
        testResults.Clear();
        for (int level = 1; level <= testLevels; level++)
        {
            for (int ai = 0; ai < 3; ai++)
            {
                int cost = balanceConfig.CalculateAbilityCost(ai, level);
                float cooldown = balanceConfig.GetCooldown(ai, level);
                float damage = cost * 2f;
                float dps = damage / cooldown;
                float timeToAfford = cost / collectionRate;

                testResults.Add(new BalanceTestResult
                {
                    level = level,
                    abilityIndex = ai,
                    timeToAfford = timeToAfford,
                    dps = dps,
                    totalCost = cost
                });
            }
        }
    }

    void AnalyzeResults()
    {
        float avgTimeIncrease = 0f, avgDPSIncrease = 0f;
        int count = 0;

        for (int i = 3; i < testResults.Count; i += 3)
        {
            float timeGrowth = testResults[i].timeToAfford / Mathf.Max(0.01f, testResults[i - 3].timeToAfford);
            float dpsGrowth  = testResults[i].dps / Mathf.Max(0.01f, testResults[i - 3].dps);
            avgTimeIncrease += timeGrowth;
            avgDPSIncrease  += dpsGrowth;
            count++;

            Debug.Log($"Уровень {testResults[i].level}: " +
                      $"Время сбора: {testResults[i].timeToAfford:F1}с (+{(timeGrowth - 1) * 100:F0}%), " +
                      $"DPS: {testResults[i].dps:F1} (+{(dpsGrowth - 1) * 100:F0}%)");
        }

        if (count > 0)
        {
            avgTimeIncrease /= count;
            avgDPSIncrease  /= count;
            Debug.Log($"Средний рост времени: {(avgTimeIncrease - 1) * 100:F1}% за уровень");
            Debug.Log($"Средний рост DPS: {(avgDPSIncrease - 1) * 100:F1}% за уровень");

            if (avgTimeIncrease > 1.3f)
                Debug.LogWarning("[BalanceTester] Время сбора растет слишком быстро!");
            if (avgDPSIncrease < 1.1f)
                Debug.LogWarning("[BalanceTester] DPS растет слишком медленно!");
        }

        // Комбо-эффективность
        for (int chain = 2; chain <= 4; chain++)
        {
            float bonus = balanceConfig.CalculateComboBonus(chain);
            float efficiency = balanceConfig.CalculateComboEfficiency(chain);
            Debug.Log($"[Комбо x{chain}] Бонус: {bonus:F2}x, Эффективность: {efficiency:F3}");
        }
    }

    void ExportToCSV()
    {
        string csv = "Level,AbilityIndex,TimeToAfford,DPS,TotalCost\n";
        foreach (var r in testResults)
            csv += $"{r.level},{r.abilityIndex},{r.timeToAfford:F2},{r.dps:F2},{r.totalCost}\n";

        string path = Application.dataPath + "/BalanceTest.csv";
        System.IO.File.WriteAllText(path, csv);
        Debug.Log($"[BalanceTester] CSV сохранён: {path}");
    }
}
