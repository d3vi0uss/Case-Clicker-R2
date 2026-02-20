const SUPABASE_URL = "https://pwzfmrzlqsxqifxfqdes.supabase.co";
const SUPABASE_ANON_KEY = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InB3emZtcnpscXN4cWlmeGZxZGVzIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NzE2MTQzNTksImV4cCI6MjA4NzE5MDM1OX0.fpIySjKRLf4KNvPDrUoeTioRhlRGpROx9j5uV2ENrnU";

window.supabaseClient = window.supabase.createClient(
  SUPABASE_URL,
  SUPABASE_ANON_KEY
);
