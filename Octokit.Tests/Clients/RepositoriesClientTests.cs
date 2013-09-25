using System;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using Octokit.Clients;
using Octokit.Http;
using Octokit.Tests.Helpers;
using Xunit;

namespace Octokit.Tests.Clients
{
    /// <summary>
    /// Client tests mostly just need to make sure they call the IApiConnection with the correct 
    /// relative Uri. No need to fake up the response. All *those* tests are in ApiConnectionTests.cs.
    /// </summary>
    public class RepositoriesClientTests
    {
        public class TheConstructor
        {
            [Fact]
            public void EnsuresNonNullArguments()
            {
                Assert.Throws<ArgumentNullException>(() => new RepositoriesClient(null));
            }
        }

        public class TheGetMethod
        {
            [Fact]
            public void RequestsCorrectUrl()
            {
                var client = Substitute.For<IApiConnection<Repository>>();
                var repositoriesClient = new RepositoriesClient(client);

                repositoriesClient.Get("fake", "repo");

                client.Received().Get(Arg.Is<Uri>(u => u.ToString() == "/repos/fake/repo"), null);
            }

            [Fact]
            public async Task EnsuresNonNullArguments()
            {
                var repositoriesClient = new RepositoriesClient(Substitute.For<IApiConnection<Repository>>());

                await AssertEx.Throws<ArgumentNullException>(async () => await repositoriesClient.Get(null, "name"));
                await AssertEx.Throws<ArgumentNullException>(async () => await repositoriesClient.Get("owner", null));
            }
        }

        public class TheGetAllForCurrentMethod
        {
            [Fact]
            public void RequestsTheCorrectUrlAndReturnsOrganizations()
            {
                var client = Substitute.For<IApiConnection<Repository>>();
                var repositoriesClient = new RepositoriesClient(client);

                repositoriesClient.GetAllForCurrent();

                client.Received()
                    .GetAll(Arg.Is<Uri>(u => u.ToString() == "user/repos"), null);
            }
        }

        public class TheGetAllForUserMethod
        {
            [Fact]
            public void RequestsTheCorrectUrlAndReturnsOrganizations()
            {
                var client = Substitute.For<IApiConnection<Repository>>();
                var repositoriesClient = new RepositoriesClient(client);

                repositoriesClient.GetAllForUser("username");

                client.Received()
                    .GetAll(Arg.Is<Uri>(u => u.ToString() == "/users/username/repos"), null);
            }

            [Fact]
            public async Task EnsuresNonNullArguments()
            {
                var reposEndpoint = new RepositoriesClient(Substitute.For<IApiConnection<Repository>>());

                AssertEx.Throws<ArgumentNullException>(async () => await reposEndpoint.GetAllForUser(null));
            }
        }

        public class TheGetAllForOrgMethod
        {
            [Fact]
            public void RequestsTheCorrectUrlAndReturnsOrganizations()
            {
                var client = Substitute.For<IApiConnection<Repository>>();
                var repositoriesClient = new RepositoriesClient(client);

                repositoriesClient.GetAllForOrg("orgname");

                client.Received()
                    .GetAll(Arg.Is<Uri>(u => u.ToString() == "/orgs/orgname/repos"), null);
            }

            [Fact]
            public void EnsuresNonNullArguments()
            {
                var reposEndpoint = new RepositoriesClient(Substitute.For<IApiConnection<Repository>>());

                AssertEx.Throws<ArgumentNullException>(async () => await reposEndpoint.GetAllForOrg(null));
            }
        }

        public class TheGetReadmeMethod
        {
            [Fact]
            public async Task ReturnsReadme()
            {
                string encodedContent = Convert.ToBase64String(Encoding.UTF8.GetBytes("Hello world"));
                var readmeInfo = new ReadmeResponse
                {
                    Content = encodedContent,
                    Encoding = "base64",
                    Name = "README.md",
                    Url = "https://github.example.com/readme.md",
                    HtmlUrl = "https://github.example.com/readme"
                };
                var client = Substitute.For<IApiConnection<Repository>>();
                client.GetItem<ReadmeResponse>(Args.Uri, null).Returns(Task.FromResult(readmeInfo));
                client.GetHtml(Args.Uri, null).Returns(Task.FromResult("<html>README</html>"));
                var reposEndpoint = new RepositoriesClient(client);

                var readme = await reposEndpoint.GetReadme("fake", "repo");

                Assert.Equal("README.md", readme.Name);
                client.Received().GetItem<ReadmeResponse>(Arg.Is<Uri>(u => u.ToString() == "/repos/fake/repo/readme"), 
                    null);
                client.DidNotReceive().GetHtml(Arg.Is<Uri>(u => u.ToString() == "https://github.example.com/readme"), 
                    null);
                var htmlReadme = await readme.GetHtmlContent();
                Assert.Equal("<html>README</html>", htmlReadme);
                client.Received().GetHtml(Arg.Is<Uri>(u => u.ToString() == "https://github.example.com/readme"), null);
            }
        }
    }
}