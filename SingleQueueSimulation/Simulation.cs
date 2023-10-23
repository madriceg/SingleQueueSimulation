using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SingleQueueSimulation
{
    
    internal class Simulation
    {
        const int Q_LIMIT = 100;
        const int BUSY = 1;
        const int IDLE = 0;


        int next_event_type, num_custs_delayed, num_delays_required, num_events, num_in_q, server_status;
        double area_num_in_q, area_server_status, mean_interarrival, mean_service, sim_time, time_last_event,
            total_of_delays;
        double[] time_next_event = new double[3] ;
        double[] time_arrival = new double[Q_LIMIT + 1];

        public void Main()
        {
            //specify the number of events for the timing function
            num_events = 2;

            //read input parameters
            mean_interarrival = 1.0;
            mean_service = .5;
            num_delays_required = 1000;

            //Write report heading input parameters
            Console.WriteLine("Single-server queuing system");
            Console.WriteLine("Mean interarrival time \t{0} minutes", mean_interarrival);
            Console.WriteLine("Mean service time \t{0} minutes", mean_service);
            Console.WriteLine("Number of customers \t{0}", num_delays_required);
            Console.WriteLine("");

            //Initialize the simulation
            Initialize();

            //Run the simulation while more delays are still needed.

            while(num_custs_delayed < num_delays_required) 
            {
                //Determine the next event
                Timing();

                //update time-average statistical accumulators
                Update_time_avg_stats();

                //Invoke the appropriate event function
                switch(next_event_type)
                {
                    case 1:
                        Arrive();
                        break;
                    case 2:
                        Depart();
                        break;
                }
            }

            //Invoke the report generator and end the simulation
            Report();

        }

        /// <summary>
        /// report generator function
        /// </summary>
        private void Report()
        {
            //compute and write estimates of desired measures of performance
            Console.WriteLine("Average delay in queue \t{0:.###} minutes  ", total_of_delays/num_custs_delayed);
            Console.WriteLine("Average number in queue {0:.###}         ", area_num_in_q / sim_time);
            Console.WriteLine("Server utilization \t{0:.###}              ", area_server_status / sim_time);
            Console.WriteLine("Time simulation ended \t{0:.###} minutes   ", sim_time);
            Console.WriteLine("");
        }

        private void Depart()
        {
            int i;
            double delay;

            //check to see whether the queue is empty
            if(num_in_q == 0)
            {
                /*
                 * the queue is empty so make the server idle and eliminate the departure (service completion)
                 * from consideration
                 */
                server_status = IDLE;
                time_next_event[2] = Math.Pow(10, 30);
            }
            else
            {
                //the queue is nonempty, so decrement the number of customers in the queue
                --num_in_q;

                /*
                 * compute the delay of the customer who is beginning service and update the total delay accumulator
                 */
                delay = sim_time - time_arrival[1];
                total_of_delays += delay;

                //increment the number of customers delayed, and schedule departure
                ++num_custs_delayed;
                time_next_event[2] = sim_time + expon(mean_service);

                //move each customer in queue (if any) up one place

                for(i = 1; i <= num_in_q; ++i)
                    time_arrival[i] = time_arrival[i + 1];
            }
        }

        private void Arrive()
        {
            double delay;
            //schedule next arrival
            time_next_event[1] = sim_time + expon(mean_interarrival);

            //check to see whether server is busy
            if(server_status == BUSY)
            {
                //server is busy, so increment number of customers in queue
                ++num_in_q;

                //check to see whether an overflow condition exists
                if(num_in_q > Q_LIMIT)
                {
                    //the queue has overflowed, so stop the simulation
                    Console.WriteLine("Overflow of the array time_arrival at {0} time", sim_time);
                }

                /*
                 * there is still room in the queue, so store the time of arrival of the 
                 * arriving customer at the (new) end of time_arrival
                 */

                time_arrival[num_in_q] = sim_time;
            }
            else
            {
                /*
                 * server is idle, so arriving customer has a delay of zero.  (The following two statements
                 * are for program clarity and do not affect the results of the simulation
                 */
                delay = 0.0;
                total_of_delays += delay;

                //increment the number of customers delayed, and make server busy
                ++num_custs_delayed;
                server_status = BUSY;

                //schedule a departure (service completion)
                time_next_event[2] = sim_time + expon(mean_service);
            }
        }

        /// <summary>
        /// update area accumulators for time-average statistics
        /// </summary>
        private void Update_time_avg_stats()
        {
            double time_since_last_event;
            //compute time since last event, and update last-event-time marker
            time_since_last_event = sim_time - time_last_event;
            time_last_event = sim_time;

            //update area under number-in-queue function
            area_num_in_q += num_in_q * time_since_last_event;

            //update area under server-busy indicator function
            area_server_status += server_status * time_since_last_event;

        }

        private void Timing()
        {
            int i;
            double min_time_next_event = Math.Pow(10, 29);
            next_event_type = 0;

            //determine the event type of the next event to occur

            for(i = 1; i <= num_events; ++i)
            {
                if (time_next_event[i] < min_time_next_event)
                {
                    min_time_next_event = time_next_event[i];
                    next_event_type = i;
                }
            }

            //check to see whether the event list is empty
            if(next_event_type == 0)
            {
                //the event list is empty, so stop the simulation
                Console.WriteLine("Event list empty at time {0}", sim_time);
                return;
            }

            //the event list is not empty, so advance the simulation clock
            sim_time = min_time_next_event;
        }

        private void Initialize()
        {
            //initialize the simulation clock
            sim_time = 0.0;

            //initialize the state variables
            server_status = IDLE;
            num_in_q = 0;
            time_last_event = 0.0;

            //initialize the statistical counters
            num_custs_delayed = 0;
            total_of_delays = 0.0;
            area_num_in_q = 0.0;
            area_server_status = 0.0;

            //Initialize event list. Since no customers are present, the departure(service completion) event is eliminated
            //from consideration
            time_next_event[1] = sim_time + expon(mean_interarrival);
            time_next_event[2] = Math.Pow(10,30);//guaranteeing that the first event will be an arrival
        }

        /// <summary>
        /// exponential variate generation function
        /// </summary>
        /// <param name="mean"></param>
        /// <returns>an exponential random variate with mean "mean"</returns>
        double expon(double mean)
        {
            return -mean * Math.Log(new Random().NextDouble());
        }
    }

   
}
