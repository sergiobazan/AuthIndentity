﻿namespace Contracts;

public sealed record LoginRequest(
    string Email, 
    string Password);
