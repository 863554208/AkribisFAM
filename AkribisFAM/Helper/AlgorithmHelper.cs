using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkribisFAM.Helper
{
    //====================================ADD BY YXW=============================
    //====================================2025.5.22==============================
    public class AlgorithmHelper
    {
        /// <summary>
        /// 基于 T 型运动轨迹，计算电机从当前位置到目标位置的预计运行时间，并根据安全系数动态设置超时时间阈值。
        /// </summary>
        /// <param name="distance">运动距离（单位：mm 或其他一致单位）</param>
        /// <param name="vel">最大速度（单位：mm/s）</param>
        /// <param name="acc">加速度（单位：mm/s²）</param>
        /// <param name="dec">减速度（单位：mm/s²）</param>
        /// <param name="timeoutFactor">超时放大系数（一般取 1.2 ~ 2.0），默认为 1 表示不放大</param>
        /// <returns>返回计算后的超时时间（单位：秒）</returns>
        public static double CalculateTmoveTimeout(double distance, double vel, double acc, double dec, double timeoutFactor = 1)
        {
            if (distance <= 0 || vel <= 0 || acc <= 0 || dec <= 0) { return 0; }

            // 计算加速段和减速段所需的位移
            double accDist = 0.5 * vel * vel / acc;
            double decDist = 0.5 * vel * vel / dec;
            double totalRampDist = accDist + decDist;

            double expectedTime;

            // 判断有匀速阶段
            if (totalRampDist < distance)
            {
                double accTime = vel / acc;
                double decTime = vel / dec;
                double uniformTime = (distance - totalRampDist) / vel;
                expectedTime = accTime + uniformTime + decTime;
            }
            else
            {
                double vMaxReachable = Math.Sqrt(distance * 2.0 * acc * dec / (acc + dec));

                double accTime = vMaxReachable / acc;
                double decTime = vMaxReachable / dec;

                expectedTime = accTime + decTime;
            }

            return expectedTime * timeoutFactor; // 返回超时时间（秒）
        }

        /// <summary>
        /// 基于五段 S 型轨迹（含匀加加速、匀加速、匀减加速等段）估算运动时间，用于设置合理超时时间。
        /// </summary>
        /// <param name="distance">目标运动距离（单位：mm 或 m，需与速度/加速度单位一致）</param>
        /// <param name="vMax">最大速度（单位：mm/s 或 m/s）</param>
        /// <param name="aMax">最大加速度（单位：mm/s² 或 m/s²）</param>
        /// <param name="jMax">最大加加速度 Jerk（单位：mm/s³ 或 m/s³）</param>
        /// <param name="timeoutFactor">超时时间安全放大因子（默认 1.5）</param>
        /// <returns>返回估算后的超时时间（单位：秒）</returns>
        public static double CalculateSmoveTimeout(double distance, double vMax, double aMax, double jMax, double timeoutFactor = 1.5)
        {
            // 1. jerk 区段持续时间：aMax = jMax * t => t = aMax / jMax
            double tJerk = aMax / jMax;

            // 2. 仅包含 jerk 的加速时间（假设匀加速段为 0）
            double tAccel = 2 * tJerk;  // 匀加加速 + 匀减加速

            // 3. 每段 jerk 的位移 s = (1/6) * j * t³（标准推导公式）
            double sJerk = (1.0 / 6.0) * jMax * Math.Pow(tJerk, 3);

            // 4. 总加速段位移 = 两个 jerk 段位移（对称）
            double sAccel = 2 * sJerk;

            // 5. 总启动+减速阶段位移（忽略匀速段）
            double sRamp = sAccel * 2;

            double expectedTime = 0;

            if (sRamp < distance)
            {
                // case1：中间包含匀速段, 剩余位移由匀速段完成
                double sCruise = distance - sRamp;

                // 匀速段时间 = s / v
                double tCruise = sCruise / vMax;

                // 总时间 = 4 段 jerk 区间 + 匀速段（两边对称，每边两个 jerk）
                expectedTime = 4 * tJerk + tCruise;
            }
            else
            {
                // case2：无法达到最大速度，无匀速阶段,用 jerk 模型近似估算------------公式为：t ≈ 2 * (s / j)^(1/3)
                expectedTime = 2 * Math.Pow(distance / jMax, 1.0 / 3);  // 粗略估算：时间 ~ s^1/3
            }

            return expectedTime * timeoutFactor;    // 返回安全放大的超时时间（秒）
        }


    }
}
