using UnityEngine;
using System.Collections.Generic;
using Phedg1Studios.Shared;
using static Phedg1Studios.TerraformWitchSpells.Util;

namespace Phedg1Studios {
    namespace TerraformWitchSpells {
        public abstract class StreamerEffectQuery :  StreamerEffectCustom {
            public abstract List<List<string>> criterias { get; }
            public abstract List<Vector3> sizes { get; }

            public abstract void OnClick(Cell cell, int criteriaIndex);

            public abstract void UpdateDataAndDisplay(List<Cell> cells, bool isClick = true);

            public abstract void RollbackData(Cell cell);

            public abstract void UpdateDisplay(Cell cell);

            public void SetupInterfaces() {
                TerraformWitchSpells.BackupSpeed();
                SpeedControlUI.inst.SetSpeed(0);
                SpeedControlUI.inst.ButtonsInteractable(false);
                witchUI.gameObject.SetActive(false);
                BuildUI.inst.SetVisible(false);
            }
        }
    }
}