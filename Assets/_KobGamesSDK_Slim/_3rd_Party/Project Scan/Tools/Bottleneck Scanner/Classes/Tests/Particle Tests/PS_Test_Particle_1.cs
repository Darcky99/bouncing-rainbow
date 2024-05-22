#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

namespace HardCodeLab.Utils.ProjectScan
{
    /// <summary>
    /// This subtest checks whether the number of maximum particles exceeds the average per Scene
    /// </summary>
    public class PS_Test_Particle_1 : PS_SubTest
    {
        /// <summary>
        /// Particle GameObjects split by their source Scene
        /// </summary>
        private List<PS_ObjectCategory> PARTICLE_CATEGORIES;

        private List<PS_Object> TargetObjects, AllParticles;

        public PS_Test_Particle_1(List<PS_Object> AllParticles)
        {
            REPORT.ResultCategory = PS_Result.CATEGORY.PARTICLE;
            ID = 13;
            TITLE = "Max Particles is above average in some Particle Systems";
            DESCRIPTION = "There are some Particle Systems which have above the average of maximum particles per Scene. This could cause an unnecessary performance loss and may also cause visual inconsistencies among particles.";
            SOLUTION = "It is advised to bring down the \"Max Particles\" down to average or lower.";

            this.AllParticles = AllParticles;

            TargetObjects = new List<PS_Object>();
            PARTICLE_CATEGORIES = new List<PS_ObjectCategory>();
        }

        /// <summary>
        /// Runs subtest
        /// </summary>
		public override void Check()
        {
            for (int i = 0; i < AllParticles.Count; i++)
            {
                GameObject psGO = (GameObject)AllParticles[i].obj;

                int resultIndex = PARTICLE_CATEGORIES.FindIndex(x => x.categoryScene == psGO.scene);

                if (resultIndex >= 0)
                {
                    PARTICLE_CATEGORIES[resultIndex].objectsList.Add(new PS_Object(psGO, PS_Object.TYPE.GAMEOBJECT));
                }
                else
                {
                    PARTICLE_CATEGORIES.Add(new PS_ObjectCategory(psGO.scene));
                    PARTICLE_CATEGORIES[PARTICLE_CATEGORIES.Count - 1].objectsList.Add(new PS_Object(psGO, PS_Object.TYPE.GAMEOBJECT));
                }
            }

            for (int i = 0; i < PARTICLE_CATEGORIES.Count; i++)
            {
                PS_ObjectCategory CATEGORY = PARTICLE_CATEGORIES[i];

                int totalParticleCount = 0;

                for (int j = 0; j < CATEGORY.objectsList.Count; j++)
                {
                    totalParticleCount += ((GameObject)CATEGORY.objectsList[i].obj).GetComponent<ParticleSystem>().main.maxParticles;
                }

                int averageParticleCount = totalParticleCount / CATEGORY.objectsList.Count;

                for (int j = 0; j < CATEGORY.objectsList.Count; j++)
                {
                    PS_Object particle = CATEGORY.objectsList[i];

                    if (((GameObject)particle.obj).GetComponent<ParticleSystem>().main.maxParticles > averageParticleCount)
                    {
                        TargetObjects.Add(particle);
                    }
                }
            }

            if (TargetObjects.Count > 0)
            {
                REPORT.hasPassed = false;
                REPORT.Populate(ID, TargetObjects, TITLE, DESCRIPTION, SOLUTION);
            }
            else
            {
                REPORT.hasPassed = true;
            }
        }
    }
}

#endif