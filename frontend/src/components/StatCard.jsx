export default function StatCard({ title, value, tone }) {
  return (
    <article className={`stat-card ${tone}`}>
      <span>{title}</span>
      <strong>{value}</strong>
    </article>
  );
}
