using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Ankylosaurus.Util
{
    public class GHC_RandomNoRepeat : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GHC_RandomNoRepeat class.
        /// </summary>
        public GHC_RandomNoRepeat()
          : base("NonConsecutive Random", "RndNoRpt",
              "Generate random integers where no consecutive integer repeats itself. If the domain length = 1, then it will function like a regular random with conecutive repetitions.",
              "Ankylosaurus", "Util")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntervalParameter("Random Range", "R", "The random domain to fit the random numbers within.", GH_ParamAccess.item, new Interval(0.0, 10.0));
            pManager.AddIntegerParameter("Number", "N", "Number of random integers", GH_ParamAccess.item, 100);
            pManager.AddIntegerParameter("Seed", "S", "Random seed for calculation", GH_ParamAccess.item, 666);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Random Numbers", "N", "The randomly generated integers that never repeat consecutively", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Interval rndRange = new Interval();
            int count = 0;
            int seed = 0;

            DA.GetData("Random Range", ref rndRange);
            DA.GetData("Number", ref count);
            DA.GetData("Seed", ref seed);

            // Check input validity
            if (rndRange.T0 >= rndRange.T1 || count <= 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid - Domain needs to be at least length 1 and count needs to be greater than 0");
                return;
            }

            // Initialize random number generator
            Random random = new Random(seed);

            // Define list to store generated numbers
            List<int> randomNumbers = new List<int>();

            // Determine if domain length is 1
            bool domainLengthOne = Math.Abs(rndRange.T1 - rndRange.T0) == 1;

            // Store previous number to ensure no consecutive repetition if necessary
            int? previousNumber = null;

            for (int i = 0; i < count; i++)
            {
                int randomNumber;

                if (domainLengthOne)
                {
                    // Just produce random numbers without consecutive checks
                    randomNumber = random.Next((int)rndRange.T0, (int)rndRange.T1 + 1);
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Since the length of the domain = 1, then there will be consecutive values because there are only 2 choices");
                }
                else
                {
                    // Ensure no consecutive repeating numbers
                    do
                    {
                        randomNumber = random.Next((int)rndRange.T0, (int)rndRange.T1 + 1);
                    }
                    while (previousNumber.HasValue && randomNumber == previousNumber.Value);
                }

                // Add the number to the list and update the previous number
                randomNumbers.Add(randomNumber);
                previousNumber = randomNumber;
            }

            // Output the list of random numbers
            DA.SetDataList("Random Numbers", randomNumbers);

        }
    

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("C4024AA1-9A7B-4A3D-9C56-08A41E7744CB"); }
        }
    }
}