using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockNotification.Service.Entity;

namespace StockNotification.Service.CheckPoint
{
    /// <summary>
    /// 移动平均线缠绕
    /// </summary>
    static class MovingLineTwine
    {
        public static string Check(int sourceCount, int targetCount, IList<Session> sessions)
        {
            if (sourceCount > targetCount)
            {
                var temp = sourceCount;
                sourceCount = targetCount;
                targetCount = temp;
            }

            if (sessions.Count == 1)
            {
                return string.Empty;
            }

            var source = new MovingLine(sourceCount, sessions);
            var target = new MovingLine(targetCount, sessions);

            var priSessions = new List<Session>();
            priSessions.AddRange(sessions);
            priSessions.RemoveAt(0);

            var priSource = new MovingLine(sourceCount, priSessions);
            var priTarget = new MovingLine(targetCount, priSessions);

            if ((source.Price > target.Price && priSource.Price < priTarget.Price)
                || (source.Price < target.Price && priSource.Price > priTarget.Price))
            {
                var upsideCount = source.Price > target.Price ? sourceCount : targetCount;
                return string.Format("{0}日均线与{1}日均线发生缠绕，{2}日均线在上",
                                     sourceCount,
                                     targetCount,
                                     upsideCount);
            }

            return string.Empty;
        }
    }
}
