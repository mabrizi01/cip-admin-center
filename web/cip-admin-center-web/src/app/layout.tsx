import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "CIP Admin Center",
  description: "Administration frontend scaffold for CIP Admin Center",
};

export default function RootLayout({ children }: Readonly<{ children: React.ReactNode }>) {
  return (
    <html lang="en">
      <body>{children}</body>
    </html>
  );
}
