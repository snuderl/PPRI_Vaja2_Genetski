using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PPRI_Vaja2_Genetski
{
    class GeneticAlgorithm<T> where T:Chromosome
    {
        public List<Generation<T>> generations = new List<Generation<T>>();
        private Trainer<T> trainer;
        private int PopulationSize;
        private double MutationProbability;
        private double CrossoverProbability;
        private int EliteSize;

        private CancellationTokenSource cts=null;
        private bool Running { get { return cts != null && !cts.IsCancellationRequested;  } }

        public GeneticAlgorithm(Trainer<T> trainer, int populationSize, double mutationProb=0.1, double crossoverProb=0.1, int eliteSize = 2, String fitness = "Vaja2"){
            this.EliteSize = eliteSize;
            this.trainer = trainer;
            this.MutationProbability = mutationProb;
            this.CrossoverProbability = crossoverProb;
            this.PopulationSize = populationSize;
            var pop = InitializePopulation(populationSize);
            Evaluate(ref pop);
            var current = new Generation<T>(pop.ToArray(), 0);
            generations.Add(current);

        }

        public List<T> InitializePopulation(int size){
            return trainer.Random().Take(size).ToList();
        }

        public void Evaluate(ref List<T> gen)
        {
            for (var i = 0; i < gen.Count; i++)
            {
                var member = gen[i];
                var fitness = trainer.Evaluate(member);
            }
            gen = gen.OrderBy(x => x.Fitness).Reverse().ToList();
        }

        public List<T> GetSurvivors(Generation<T> g, int numberSurvivors)
        {
            List<T> survivors = new List<T>();
            while (survivors.Count != numberSurvivors)
            {
                int index = trainer.SelectionAlgorithm.Select(g);
                var choosen = g.members[index];
                if(trainer.IsValid(choosen)){
                    survivors.Add(choosen);
                }
            }


            return survivors;
        }

        public void Step(int num)
        {
            for (var i = 0; i < num; i++)
            {
                Step();
            }
        }


        public void Run(Action<IEnumerable<Generation<T>>> onProgress, Action onCompleted, int progressStep = 100)
        {
            TaskScheduler uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            cts = new CancellationTokenSource();
            progressStep = 5;
            var token = cts.Token;
            Task.Factory.StartNew(() =>
            {
                var hasConverged = false;
                int i = generations.Count;
                while (i < 1000 && !cts.IsCancellationRequested && !hasConverged)
                {
                    int k = 0;
                    while (!cts.IsCancellationRequested && k < progressStep)
                    {
                        hasConverged = generations.Count > 100 && generations.Last().MaxFitness == generations[generations.Count - 100].MaxFitness;
                        if (hasConverged)
                        {
                            break;
                        }
                        Step(1);
                        k++;
                    }
                    var last10 =generations.Skip(i).ToList();
                    Task.Factory.StartNew(() => { onProgress(last10); }, CancellationToken.None, TaskCreationOptions.None, uiScheduler);
                    i += progressStep;

                }
                cts = null;
                Task.Factory.StartNew(() => { onCompleted(); }, CancellationToken.None, TaskCreationOptions.None, uiScheduler);                
            }, cts.Token);

        }

        public void Stop()
        {
            if (cts != null && !cts.IsCancellationRequested)
            {
                cts.Cancel();
            }
        }

        public void Step()
        {
            var currentGeneration = generations.Last();
            var parents = GetSurvivors(currentGeneration, PopulationSize);

            List<T> newPopulation = new List<T>();
            newPopulation.AddRange(currentGeneration.members.Take(EliteSize));
            int crossoverCount = 0;
            while (newPopulation.Count < PopulationSize)
            {
                var p1 = parents[Utility.Random.Next(parents.Count)];
                var p2 = parents[Utility.Random.Next(parents.Count)];

                var crossover = Utility.Random.NextDouble();
                if (crossover < CrossoverProbability)
                {
                    var valid = trainer.CrossoverAlgoirthm.Crossover(p1, p2).Where(x => trainer.IsValid(x));
                    newPopulation.AddRange(valid);
                    crossoverCount += valid.Count();
                }
                else
                {
                    newPopulation.Add(p1);
                    newPopulation.Add(p2);
                }
            }

            // Ce jih je prevec odstrani zadnjega
            if (newPopulation.Count > PopulationSize)
            {
                newPopulation.RemoveAt(PopulationSize);
            }


            for (int i = EliteSize; i < PopulationSize; i++)
            {
                double mutate = Utility.Random.NextDouble();
                if (mutate < MutationProbability)
                {
                    T mutiran;
                    do
                    {
                        mutiran = trainer.MutatorAlgorithm.Mutate(newPopulation[i]);
                    } while (!trainer.IsValid(mutiran));
                    newPopulation[i] = mutiran;
                }
            }

            Evaluate(ref newPopulation);
            var newGeneration = new Generation<T>(newPopulation.ToArray(), generations.Count);
            generations.Add(newGeneration);
        }
    }
}
