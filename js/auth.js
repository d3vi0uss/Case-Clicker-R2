async function signup(email, password) {
  return await supabaseClient.auth.signUp({
    email,
    password
  });
}

async function login(email, password) {
  return await supabaseClient.auth.signInWithPassword({
    email,
    password
  });
}

async function logout() {
  await supabaseClient.auth.signOut();
}
