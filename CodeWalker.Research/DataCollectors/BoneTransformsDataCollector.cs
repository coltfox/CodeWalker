using CodeWalker.Research;
using CodeWalker.Research.Utils;
using System.IO;
using SharpDX;
using System.Diagnostics;
using System.Collections.Generic;

namespace CodeWalker.GameFiles.DataCollectors
{
    public class BoneTransformsDataCollector : GameFileDataCollector
    {
        public override string TestName { get; } = "BoneTransforms";
        public int NumYftsWithUnk1TransformsIdentity = 0;
        public int NumYftsWithUnk1 = 0;
        public int NumYftsWithUnk0TransformsIdentity = 0;
        public int NumYftsWithUnk0 = 0;
        public bool BoneTransOnlyPresentWithDrawableArray = true;
        public HashSet<int> PossibleUnkValues = new HashSet<int>();

        public override bool IsValidEntry(RpfEntry entry)
        {
            return entry.Path.EndsWith(".yft");
        }

        public override void HandleEntry(RpfEntry entry)
        {
            YftFile yft;

            try
            {
                yft = RpfMan.GetFile<YftFile>(entry);
            }
            catch 
            {
                return;
            }

            FragBoneTransforms boneTransforms = yft.Fragment?.BoneTransforms;

            if (boneTransforms == null) return;

            bool allMatricesIdentity = AreMatricesAllIdentity(boneTransforms.Items);
            int unk = boneTransforms.Unknown_12h;
            PossibleUnkValues.Add(boneTransforms.Unknown_12h);

            if (unk == 1)
            {
                NumYftsWithUnk1++;

                if (allMatricesIdentity)
                {
                    NumYftsWithUnk1TransformsIdentity++;
                }
                else
                {
                    Trace.WriteLine(string.Format("{0} has BoneTransforms with unk=1 and some matrices are NOT identity!", Path.GetFileName(entry.Path)));
                }

                if (yft.Fragment?.DrawableArray != null && yft.Fragment.DrawableArray.Count > 0)
                {
                    BoneTransOnlyPresentWithDrawableArray = false;
                }
            }

            if (unk == 0)
            {
                NumYftsWithUnk0++;

                if (allMatricesIdentity && boneTransforms.Items.Length > 1) NumYftsWithUnk0TransformsIdentity++;

                if (yft.Fragment?.DrawableArray == null || yft.Fragment.DrawableArray.Count == 0)
                {
                    BoneTransOnlyPresentWithDrawableArray = false;
                }
            }
        }

        public bool AreMatricesAllIdentity(Matrix3_s[] matrices)
        {
            foreach (Matrix3_s matrix in matrices)
            {
                Matrix matrixDx = Matrix3_sToMatrix4(matrix);
                if (!ResearchUtils.matsNearEqual(matrixDx, Matrix.Identity, 0.001)) return false;
            }

            return true;
        }

        public Matrix Matrix3_sToMatrix4(Matrix3_s m)
        {
            return new Matrix(
                m.Row1.X, m.Row1.Y, m.Row1.Z, m.Row1.W,
                m.Row2.X, m.Row2.Y, m.Row2.Z, m.Row2.W,
                m.Row3.X, m.Row3.Y, m.Row3.Z, m.Row3.W,
                0.0f, 0.0f, 0.0f, 1.0f
            );
        }

        public override void OnEnd(int numFilesTested)
        {
            double ratioUnk1 = (double) NumYftsWithUnk1TransformsIdentity / NumYftsWithUnk1;
            double ratioUnk0 = (double) NumYftsWithUnk0TransformsIdentity / NumYftsWithUnk0;

            Trace.WriteLine(string.Format("Possible BoneTransforms unk values: {0}", string.Join(",", PossibleUnkValues)));
            Trace.WriteLine(string.Format("In {0:P3} of yfts with BoneTransforms unk=1, all BoneTransforms were identity matrices", ratioUnk1));
            Trace.WriteLine(string.Format("In {0:P3} of yfts with BoneTransforms unk=0, all BoneTransforms were identity matrices", ratioUnk0));

            if (ratioUnk1 == 1.0 && NumYftsWithUnk0TransformsIdentity == 0)
            {
                Trace.WriteLine("All BoneTransforms with unk=1 have identity matrices and this is never the case with BoneTransforms unk=0. It's safe to say that unk=1 must disable BoneTransforms.");
            }

            if (BoneTransOnlyPresentWithDrawableArray)
            {
                Trace.WriteLine("In all yfts, BoneTransforms are all identity matrices UNLESS there is a DrawableArray. Are BoneTransforms separate transforms for the damaged drawable?");
            }
            else
            {
                Trace.WriteLine("In all yfts, BoneTransforms are present regardless of DrawableArray being present.");
            }

            base.OnEnd(numFilesTested);
        }


    }
}
