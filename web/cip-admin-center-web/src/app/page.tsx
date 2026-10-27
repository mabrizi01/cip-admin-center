"use client";

import { FormEvent, useEffect, useState } from "react";
import { createCustomer, Customer, getHealth } from "@/lib/api";

// Scaffold only: demonstrates how the admin UI consumes the API (health + one use case).
export default function Home() {
  const [health, setHealth] = useState("checking…");
  const [name, setName] = useState("");
  const [customer, setCustomer] = useState<Customer | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    getHealth().then(setHealth).catch(() => setHealth("unreachable"));
  }, []);

  async function onSubmit(event: FormEvent) {
    event.preventDefault();
    setError(null);
    try {
      setCustomer(await createCustomer(name));
    } catch (e) {
      setError(e instanceof Error ? e.message : "Unexpected error");
    }
  }

  return (
    <main style={{ maxWidth: 640, margin: "2rem auto", fontFamily: "sans-serif" }}>
      <h1>CIP Admin Center</h1>
      <p>API status: {health}</p>
      <h2>Create a Customer</h2>
      <form onSubmit={onSubmit}>
        <input value={name} onChange={(e) => setName(e.target.value)} placeholder="Customer name" required />
        <button type="submit">Create</button>
      </form>
      {error && <p role="alert">{error}</p>}
      {customer && <p>Created {customer.name} ({customer.id})</p>}
    </main>
  );
}
