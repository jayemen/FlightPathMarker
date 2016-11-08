using VRageMath;

namespace FlightPathMarker
{
    public class AveragedVector
    {
        private readonly Vector3[] samples;
        private int nextSample = 0;
        
        public AveragedVector(int numSamples) {
            this.samples = new Vector3[numSamples];
        }

        public Vector3 Update(Vector3 velocity)
        {
            samples[nextSample] = velocity;
            nextSample = (nextSample + 1) % samples.Length;

            var average = samples[0];

            for (var i = 1; i < samples.Length; ++i)
            {
                average += samples[i];
            }

            return average / samples.Length;
        }
    }
}