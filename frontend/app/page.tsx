const stats = [
  { label: 'Jobs', value: '12', detail: '3 active schedules' },
  { label: 'Executions today', value: '48', detail: '41 succeeded, 7 failed' },
  { label: 'Retry rate', value: '14.6%', detail: 'Within expected range' },
  { label: 'Next run', value: '06:15 UTC', detail: 'Invoice sync job' },
];

const jobs = [
  { name: 'Daily invoice sync', status: 'Enabled', cadence: 'Every 60 min', lastRun: '2 min ago', outcome: 'Succeeded' },
  { name: 'CRM enrichment', status: 'Enabled', cadence: 'Every 15 min', lastRun: '8 min ago', outcome: 'Failed' },
  { name: 'Payroll export', status: 'Paused', cadence: 'Manual only', lastRun: 'Yesterday', outcome: 'Succeeded' },
  { name: 'Webhook fanout', status: 'Enabled', cadence: 'Every 5 min', lastRun: 'Now', outcome: 'Running' },
];

const events = [
  'Worker claimed execution #018f1c and started attempt 2.',
  'Retry queued for CRM enrichment after a transient 502 response.',
  'Heartbeat updated for webhook fanout during active execution.',
  'Scheduler advanced next run time for daily invoice sync.',
];

export default function Home() {
  return (
    <main className="min-h-screen text-slate-100">
      <section className="mx-auto flex min-h-screen max-w-7xl flex-col gap-8 px-4 py-6 sm:px-6 lg:px-8">
        <header className="overflow-hidden rounded-3xl border border-white/10 bg-white/5 shadow-glow backdrop-blur-xl">
          <div className="dashboard-grid flex flex-col gap-6 border-b border-white/10 p-6 lg:flex-row lg:items-end lg:justify-between lg:p-8">
            <div className="max-w-3xl space-y-4">
              <p className="font-mono text-xs uppercase tracking-[0.3em] text-emerald-300/80">Enrichly HR</p>
              <h1 className="text-3xl font-semibold tracking-tight text-white sm:text-5xl">
                Job Automation Platform
              </h1>
              <p className="max-w-2xl text-sm leading-7 text-slate-300 sm:text-base">
                A focused dashboard foundation for managing automated jobs, monitoring execution history,
                and preparing the queue-driven backend that powers scheduling and retries.
              </p>
            </div>

            <div className="grid gap-3 sm:grid-cols-3 lg:min-w-[26rem]">
              <button className="rounded-2xl border border-emerald-400/20 bg-emerald-400/10 px-4 py-3 text-left text-sm font-medium text-emerald-100 transition hover:bg-emerald-400/15">
                Create job
              </button>
              <button className="rounded-2xl border border-white/10 bg-white/5 px-4 py-3 text-left text-sm font-medium text-slate-100 transition hover:bg-white/10">
                Run now
              </button>
              <button className="rounded-2xl border border-white/10 bg-white/5 px-4 py-3 text-left text-sm font-medium text-slate-100 transition hover:bg-white/10">
                View history
              </button>
            </div>
          </div>

          <div className="grid gap-px bg-white/5 sm:grid-cols-2 xl:grid-cols-4">
            {stats.map((stat) => (
              <article key={stat.label} className="bg-[#09111f] p-6">
                <p className="text-sm text-slate-400">{stat.label}</p>
                <div className="mt-4 flex items-end justify-between gap-4">
                  <p className="text-3xl font-semibold text-white">{stat.value}</p>
                  <p className="text-xs uppercase tracking-[0.2em] text-emerald-300/80">Live</p>
                </div>
                <p className="mt-3 text-sm leading-6 text-slate-400">{stat.detail}</p>
              </article>
            ))}
          </div>
        </header>

        <div className="grid gap-6 xl:grid-cols-[1.7fr_1fr]">
          <section className="overflow-hidden rounded-3xl border border-white/10 bg-white/5 shadow-glow backdrop-blur-xl">
            <div className="flex items-center justify-between border-b border-white/10 px-6 py-5">
              <div>
                <h2 className="text-lg font-semibold text-white">Jobs</h2>
                <p className="text-sm text-slate-400">Responsive table layout for definition, schedule, and status.</p>
              </div>
              <span className="rounded-full border border-white/10 bg-white/5 px-3 py-1 text-xs font-medium text-slate-300">
                PostgreSQL-backed
              </span>
            </div>

            <div className="overflow-x-auto">
              <table className="min-w-full divide-y divide-white/10 text-left">
                <thead className="bg-white/5 text-xs uppercase tracking-[0.2em] text-slate-400">
                  <tr>
                    <th className="px-6 py-4 font-medium">Name</th>
                    <th className="px-6 py-4 font-medium">Status</th>
                    <th className="px-6 py-4 font-medium">Cadence</th>
                    <th className="px-6 py-4 font-medium">Last run</th>
                    <th className="px-6 py-4 font-medium">Outcome</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-white/10">
                  {jobs.map((job) => (
                    <tr key={job.name} className="transition hover:bg-white/5">
                      <td className="px-6 py-4">
                        <div className="font-medium text-white">{job.name}</div>
                        <div className="text-sm text-slate-400">Execution history ready</div>
                      </td>
                      <td className="px-6 py-4 text-sm text-slate-300">{job.status}</td>
                      <td className="px-6 py-4 text-sm text-slate-300">{job.cadence}</td>
                      <td className="px-6 py-4 text-sm text-slate-300">{job.lastRun}</td>
                      <td className="px-6 py-4 text-sm">
                        <span className={`rounded-full px-3 py-1 ${job.outcome === 'Failed' ? 'bg-rose-500/15 text-rose-200' : job.outcome === 'Running' ? 'bg-sky-500/15 text-sky-200' : 'bg-emerald-500/15 text-emerald-200'}`}>
                          {job.outcome}
                        </span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </section>

          <aside className="space-y-6">
            <section className="rounded-3xl border border-white/10 bg-white/5 p-6 shadow-glow backdrop-blur-xl">
              <h2 className="text-lg font-semibold text-white">Execution timeline</h2>
              <div className="mt-5 space-y-4">
                {events.map((event, index) => (
                  <div key={event} className="flex gap-4">
                    <div className="flex w-6 shrink-0 justify-center">
                      <div className="mt-1 h-2.5 w-2.5 rounded-full bg-emerald-300" />
                    </div>
                    <div className="pb-4">
                      <p className="text-sm leading-6 text-slate-300">{event}</p>
                      <p className="mt-1 text-xs uppercase tracking-[0.2em] text-slate-500">Event {index + 1}</p>
                    </div>
                  </div>
                ))}
              </div>
            </section>

            <section className="rounded-3xl border border-amber-400/20 bg-amber-400/10 p-6 text-amber-50 shadow-glow backdrop-blur-xl">
              <p className="text-xs uppercase tracking-[0.3em] text-amber-200/80">Foundation checklist</p>
              <h2 className="mt-3 text-xl font-semibold text-white">What starts cleanly</h2>
              <ul className="mt-4 space-y-3 text-sm leading-6 text-amber-50/90">
                <li>Frontend dashboard shell with Tailwind styling.</li>
                <li>API project with Swagger and database migration startup.</li>
                <li>Separate worker process ready for queue polling.</li>
                <li>PostgreSQL container wired into Docker Compose.</li>
              </ul>
            </section>
          </aside>
        </div>
      </section>
    </main>
  );
}
