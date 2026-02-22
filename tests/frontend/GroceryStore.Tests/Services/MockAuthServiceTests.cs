using FluentAssertions;
using GroceryStore.Services.Mock;

namespace GroceryStore.Tests.Services;

public class MockAuthServiceTests
{
    private MockAuthService NewSut()
    {
        return new( );
    }


    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        var sut = NewSut( );

        var token = await sut.LoginAsync("admin","admin123");

        token.Should( ).NotBeNullOrWhiteSpace( );
    }

    [Fact]
    public async Task Login_ValidCredentials_SetsIsAuthenticatedTrue()
    {
        var sut = NewSut( );

        await sut.LoginAsync("admin","admin123");

        sut.IsAuthenticated.Should( ).BeTrue( );
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsNull()
    {
        var sut = NewSut( );

        var token = await sut.LoginAsync("admin","wrongpassword");

        token.Should( ).BeNull( );
    }

    [Fact]
    public async Task Login_WrongUsername_ReturnsNull()
    {
        var sut = NewSut( );

        var token = await sut.LoginAsync("notadmin","admin123");

        token.Should( ).BeNull( );
    }

    [Fact]
    public async Task Login_WrongCredentials_LeavesIsAuthenticatedFalse()
    {
        var sut = NewSut( );

        await sut.LoginAsync("admin","wrong");

        sut.IsAuthenticated.Should( ).BeFalse( );
    }

    [Fact]
    public async Task Login_EmptyCredentials_ReturnsNull()
    {
        var sut = NewSut( );

        var token = await sut.LoginAsync(string.Empty,string.Empty);

        token.Should( ).BeNull( );
    }

    // ── LogoutAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task Logout_AfterLogin_SetsIsAuthenticatedFalse()
    {
        var sut = NewSut( );
        await sut.LoginAsync("admin","admin123");

        await sut.LogoutAsync( );

        sut.IsAuthenticated.Should( ).BeFalse( );
    }

    [Fact]
    public async Task Logout_WithoutLogin_DoesNotThrow()
    {
        var sut = NewSut( );

        var act = async () => await sut.LogoutAsync( );

        await act.Should( ).NotThrowAsync( );
    }

    // ── AuthStateChanged event ────────────────────────────────────────────────

    [Fact]
    public async Task Login_ValidCredentials_RaisesAuthStateChangedEvent()
    {
        var sut = NewSut( );
        var raised = false;
        sut.AuthStateChanged += () => raised = true;

        await sut.LoginAsync("admin","admin123");

        raised.Should( ).BeTrue( );
    }

    [Fact]
    public async Task Login_InvalidCredentials_DoesNotRaiseAuthStateChangedEvent()
    {
        var sut = NewSut( );
        var raised = false;
        sut.AuthStateChanged += () => raised = true;

        await sut.LoginAsync("admin","wrong");

        raised.Should( ).BeFalse( );
    }

    [Fact]
    public async Task Logout_RaisesAuthStateChangedEvent()
    {
        var sut = NewSut( );
        await sut.LoginAsync("admin","admin123");
        var raised = false;
        sut.AuthStateChanged += () => raised = true;

        await sut.LogoutAsync( );

        raised.Should( ).BeTrue( );
    }

    // ── Login → Logout → Login cycle ─────────────────────────────────────────

    [Fact]
    public async Task LoginAfterLogout_CanAuthenticateAgain()
    {
        var sut = NewSut( );
        await sut.LoginAsync("admin","admin123");
        await sut.LogoutAsync( );

        var token = await sut.LoginAsync("admin","admin123");

        token.Should( ).NotBeNull( );
        sut.IsAuthenticated.Should( ).BeTrue( );
    }
}
